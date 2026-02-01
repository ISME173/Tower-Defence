using _Project.Scripts.Castle;
using _Project.Scripts.EnemiesManagement.Spawn;
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

        private readonly Subject<Unit> LevelCompleted, LevelFailed;

        public void Initialize(CastleHealthManagement castleHealthManagement, EnemiesSpawner enemysSpawner)
        {
            _castleHealthManagement = castleHealthManagement;
            _enemysSpawner = enemysSpawner;

            _castleHealthManagement.ReadOnlyCastleDestroyed.Subscribe(OnCastleDestroyed).AddTo(Disposables);
            _enemysSpawner.ReadOnlyAllEnemiedDefeated.Subscribe(OnAllEnemiesDefeated).AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables?.Dispose();
        }

        private void OnAllEnemiesDefeated(Unit unit)
        {
            Debug.Log("<color=greeb>Current level completed!</color>");
            LevelCompleted?.OnNext(Unit.Default);
        }

        private void OnCastleDestroyed(Unit unit)
        {
            Debug.Log($"<color=red>Current level failed!</color>");
            LevelFailed?.OnNext(Unit.Default);
        }
    }
}
