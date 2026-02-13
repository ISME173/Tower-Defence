using _Project.Scripts.Construction;
using _Project.Scripts.EnemiesManagement;
using _Project.Scripts.EnemiesManagement.Spawn;
using _Project.Scripts.LevelsManagement;
using R3;
using System;
using UnityEngine;

namespace _Project.Scripts.MoneySystem
{
    public class MoneyManagement : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();
        private readonly ReactiveProperty<int> CurrentAmountOfMoney = new();

        private EnemiesSpawner _enemiesSpawner;
        private LevelsCreator _levelCreator;


        private readonly Subject<Unit> OnMoneysAdded = new Subject<Unit>();

        public Observable<Unit> ReadOnlyOnMoneysAdded => OnMoneysAdded;
        public ReadOnlyReactiveProperty<int> ReadOnlyCurrentAmountOfMoney => CurrentAmountOfMoney;

        public void Initialze(EnemiesSpawner enemiesSpawner, LevelsCreator levelsCreator)
        {
            _enemiesSpawner = enemiesSpawner;
            _levelCreator = levelsCreator;

            _enemiesSpawner.ReadOnlyEnemyDied
                .Subscribe(OnEnemyDied)
                .AddTo(Disposables);

            _levelCreator.LevelCreated
                .Subscribe(OnLevelCreated)
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables?.Dispose();
        }

        public void OnDestroyedTower(Tower tower)
        {
            int refundAfterDestruction = Math.Max(0, tower.RefundAfterDestruction);

            CurrentAmountOfMoney.Value += refundAfterDestruction;
            OnMoneysAdded?.OnNext(Unit.Default);
        }

        public bool TryGetMoneyForBuildTower(Tower towerPrefab)
        {
            if (towerPrefab == null)
                return false;

            int towerBuildPrice = towerPrefab.BuildPrice;

            if (towerBuildPrice < 0)
            {
                Debug.LogError($"Tower '{towerPrefab.gameObject.name}' build price < 0: {towerBuildPrice}");
                return false;
            }

            if (CurrentAmountOfMoney.CurrentValue >= towerBuildPrice)
            {
                CurrentAmountOfMoney.Value -= towerBuildPrice;
                return true;
            }
            
            return false;
        }

        public bool TryGetMoneyForUpgradeTower(Tower tower)
        {
            if (tower == null)
                return false;

            int towerBuildPrice = tower.GetBuildPriceForNextLevel();

            if (towerBuildPrice < 0)
            {
                Debug.LogError($"Tower '{tower.gameObject.name}' build price < 0: {towerBuildPrice}");
                return false;
            }

            if (CurrentAmountOfMoney.CurrentValue >= towerBuildPrice)
            {
                CurrentAmountOfMoney.Value -= towerBuildPrice;
                return true;
            }

            return false;
        }

        private void OnLevelCreated(LevelObject levelObject)
        {
            CurrentAmountOfMoney.Value = levelObject.InitialAmountOfMoney;
        }

        private void OnEnemyDied(Enemy enemy)
        {
            int moneyForMurderEnemy = enemy.MoneyForMurder;

            if (moneyForMurderEnemy < 0)
            {
                Debug.LogError($"Money for murder enemy '{enemy.EnemyName}' < 0: {moneyForMurderEnemy}");
                return;
            }

            CurrentAmountOfMoney.Value += moneyForMurderEnemy;
            OnMoneysAdded?.OnNext(Unit.Default);
        }
    }
}
