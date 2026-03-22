using _Project.Scripts.Advertisement;
using _Project.Scripts.Audio;
using _Project.Scripts.CameraControll;
using _Project.Scripts.EnemiesManagement;
using _Project.Scripts.EnemiesManagement.Spawn;
using _Project.Scripts.LevelsManagement;
using R3;
using System;
using UnityEngine;

namespace _Project.Scripts.Castle
{
    public class CastleHealthManagement
    {
        private readonly CastleHealthView CastleHealthView;
        private readonly CompositeDisposable Disposables = new();

        private EnemiesSpawner _enemysSpawner;
        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelCompletionManagement;
        private CameraMoving _cameraMoving;
        private int _maxHealth;
        private int _addHeartsCountAfterWatchAdv;

        private IAudioService _audioService;
        private AudioEvent _buttonClickAudioEvent;

        private IAdvertisement _advertisement;

        private readonly Subject<Unit> CastleDestroyed = new();
        private readonly Subject<int> CastleDamageTaken = new();
        private readonly ReactiveProperty<int> CurrentHealth = new();

        public Observable<Unit> ReadOnlyCastleDestroyed => CastleDestroyed;
        public Observable<int> ReadOnlyCastleDamageTaken => CastleDamageTaken;
        public ReadOnlyReactiveProperty<int> ReadOnlyCurrentHealth => CurrentHealth;

        public CastleHealthManagement(CastleHealthView castleHealthView, int addHeartsCountAfterWatchAdv)
        {
            CastleHealthView = castleHealthView;

            CastleHealthView.ReadOnlyOnGetHeartsButtonClicked
                .Subscribe(OnGetHeartsButtonClicked)
                .AddTo(Disposables);

            CastleHealthView.ReadOnlyOnWatchAdvButtonClicked
                .Subscribe(OnWatchAdvForGetHeartsButtonClicked)
                .AddTo(Disposables);

            CastleHealthView.ReadOnlyOnNoWatchAdvButtonClicked
                .Subscribe(OnNoWatchAdvForGetHeartsButtonClicked)
                .AddTo(Disposables);

            _addHeartsCountAfterWatchAdv = addHeartsCountAfterWatchAdv;
        }

        public void Initialize(EnemiesSpawner enemysSpawner, LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement,
            IAdvertisement advertisement, IAudioService audioService, AudioEvent buttonClickAudioEvent, CameraMoving cameraMoving)
        {
            _enemysSpawner = enemysSpawner;
            _levelsCreator = levelsCreator;
            _levelCompletionManagement = levelCompletionManagement;
            _audioService = audioService;
            _buttonClickAudioEvent = buttonClickAudioEvent;
            _advertisement = advertisement;
            _cameraMoving = cameraMoving;

            _levelCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(_ =>
                {
                    CastleHealthView.HideMainView();
                    CastleHealthView.HideWatchAdvForGetHeartsPanel();
                })
                .AddTo(Disposables);

            _levelCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(_ =>
                {
                    CastleHealthView.HideMainView();
                    CastleHealthView.HideWatchAdvForGetHeartsPanel();
                })
                .AddTo(Disposables);

            Disposables.Add(_levelsCreator.ReadOnlyLevelCreated.Subscribe(OnLevelCreated));
            Disposables.Add(_enemysSpawner.ReadOnlyEnemyMovedToLastPoint.Subscribe(OnEnemyMovedToLastPoint));
        }

        public void Dispose()
        {
            Disposables?.Dispose();
            CastleDestroyed?.Dispose();
        }

        private void OnGetHeartsButtonClicked(Unit unit)
        {
            _cameraMoving.LockMoving();
            Time.timeScale = 0;

            _audioService.PlayOneShot(_buttonClickAudioEvent);
            CastleHealthView.ShowWatchAdvForGetHeartsPanel();
        }

        private void OnNoWatchAdvForGetHeartsButtonClicked(Unit unit)
        {
            _cameraMoving.UnlockMoving();
            Time.timeScale = 1;

            _audioService.PlayOneShot(_buttonClickAudioEvent);
            CastleHealthView.HideWatchAdvForGetHeartsPanel();
        }

        private void OnWatchAdvForGetHeartsButtonClicked(Unit unit)
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);

            _advertisement?.ShowRewardedAdv(() =>
            {
                CurrentHealth.Value += _addHeartsCountAfterWatchAdv;
                CastleHealthView.HideWatchAdvForGetHeartsPanel();

                _cameraMoving.UnlockMoving();
                Time.timeScale = 1;
            });
        }

        private void OnLevelCreated(LevelObject levelObject)
        {
            _maxHealth = Math.Max(1, levelObject.MaxLevelHealth);
            CurrentHealth.Value = _maxHealth;

            CastleHealthView.ShowMainView();
        }

        private void OnEnemyMovedToLastPoint(Enemy enemy)
        {
            if (CurrentHealth.CurrentValue == 0)
                return;

            int enemyAttackDamage = enemy.AttackDamage;

            int finalDamage = Math.Clamp(enemyAttackDamage, 0, CurrentHealth.CurrentValue);
            CurrentHealth.Value -= finalDamage;

            CastleDamageTaken.OnNext(CurrentHealth.CurrentValue);

            if (CurrentHealth.CurrentValue == 0)
            {
                CastleDestroyed.OnNext(Unit.Default);
            }
        }
    }
}
