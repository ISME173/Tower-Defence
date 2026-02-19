using _Project.Scripts.CameraControll;
using _Project.Scripts.Castle;
using _Project.Scripts.EnemiesManagement.Spawn;
using Cysharp.Threading.Tasks;
using R3;
using System;
using UnityEngine;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelCompletionManagement : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();

        private CastleHealthManagement _castleHealthManagement;
        private EnemiesSpawner _enemysSpawner;
        private LevelsCreator _levelsCreator;
        private CameraMoving _cameraMoving;

        private readonly Subject<Unit> LevelCompleted = new(), LevelFailed = new();

        public Observable<Unit> ReadOnlyLevelCompleted => LevelCompleted;
        public Observable<Unit> ReadOnlyLevelFailed => LevelFailed;

        private bool _isFinished;

        public void Initialize(CastleHealthManagement castleHealthManagement, EnemiesSpawner enemysSpawner, LevelsCreator levelsCreator, CameraMoving cameraMoving)
        {
            _castleHealthManagement = castleHealthManagement;
            _enemysSpawner = enemysSpawner;
            _levelsCreator = levelsCreator;
            _cameraMoving = cameraMoving;

            _isFinished = false;

            // Сброс состояния на новый уровень (иначе после первого завершения больше ничего не завершится)
            _levelsCreator.ReadOnlyLevelCreated
                .Subscribe(_ => _isFinished = false)
                .AddTo(Disposables);

            _castleHealthManagement.ReadOnlyCastleDestroyed
                .Subscribe(OnCastleDestroyed)
                .AddTo(Disposables);

            _enemysSpawner.ReadOnlyAllEnemiedDefeated
                .Subscribe(OnAllEnemiesDefeated)
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables?.Dispose();
        }

        private async void OnAllEnemiesDefeated(Unit unit)
        {
            // ВАЖНО: AllEnemiesDefeated может прийти до того, как CastleHealthManagement снимет HP
            // в обработчике EnemyMovedToLastPoint. Ждём 1 кадр, чтобы HP успело обновиться.
            await UniTask.Yield();

            if (_isFinished)
                return;

            int hp = _castleHealthManagement.ReadOnlyCurrentHealth.CurrentValue;

            if (hp > 0)
            {
                FinishCompleted();
            }
            else
            {
                FinishFailed();
            }

            _cameraMoving.LockMoving();
        }

        private void OnCastleDestroyed(Unit unit)
        {
            FinishFailed();
        }

        private void FinishCompleted()
        {
            if (_isFinished)
                return;

            _isFinished = true;
            Debug.Log("<color=green>Current level completed!</color>");
            LevelCompleted?.OnNext(Unit.Default);
        }

        private void FinishFailed()
        {
            if (_isFinished)
                return;

            _isFinished = true;
            Debug.Log("<color=red>Current level failed!</color>");
            LevelFailed?.OnNext(Unit.Default);
        }
    }
}
