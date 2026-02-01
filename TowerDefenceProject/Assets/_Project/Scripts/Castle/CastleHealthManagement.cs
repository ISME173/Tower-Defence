using _Project.Scripts.EnemiesManagement;
using _Project.Scripts.EnemiesManagement.Spawn;
using _Project.Scripts.LevelsManagement;
using R3;
using System;

namespace _Project.Scripts.Castle
{
    public class CastleHealthManagement
    {
        private readonly CompositeDisposable Disposables = new();

        private EnemiesSpawner _enemysSpawner;
        private LevelsCreator _levelsCreator;
        private int _maxHealth;

        private readonly Subject<Unit> CastleDestroyed = new();
        private readonly Subject<int> CastleDamageTaken = new();
        private readonly ReactiveProperty<int> CurrentHealth = new();

        public Observable<Unit> ReadOnlyCastleDestroyed => CastleDestroyed;
        public Observable<int> ReadOnlyCastleDamageTaken => CastleDamageTaken;
        public ReadOnlyReactiveProperty<int> ReadOnlyCurrentHealth => CurrentHealth;
        public int MaxHealth => _maxHealth;

        public void Initialize(EnemiesSpawner enemysSpawner, LevelsCreator levelsCreator)
        {
            _enemysSpawner = enemysSpawner;
            _levelsCreator = levelsCreator;

            Disposables.Add(_levelsCreator.LevelCreated.Subscribe(OnLevelCreated));
            Disposables.Add(_enemysSpawner.ReadOnlyEnemyMovedToLastPoint.Subscribe(OnEnemyMovedToLastPoint));
        }

        public void Dispose()
        {
            Disposables?.Dispose();
            CastleDestroyed?.Dispose();
        }

        private void OnLevelCreated(LevelObject levelObject)
        {
            _maxHealth = Math.Max(1, levelObject.MaxLevelHealth);
            CurrentHealth.Value = _maxHealth;
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
