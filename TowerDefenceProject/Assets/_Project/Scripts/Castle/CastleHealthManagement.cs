using _Project.Scripts.EnemiesManagement;
using _Project.Scripts.EnemiesManagement.Spawn;
using _Project.Scripts.LevelsManagement;
using R3;
using System;

namespace _Project.Scripts.Castle
{
    public class CastleHealthManagement
    {
        private readonly CastleHealthView CastleHealthView;
        private readonly CompositeDisposable Disposables = new();

        private EnemiesSpawner _enemysSpawner;
        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelCompletionManagement;
        private int _maxHealth;
        private int _addHeartsCountAfterWatchAdv;

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

        public void Initialize(EnemiesSpawner enemysSpawner, LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement)
        {
            _enemysSpawner = enemysSpawner;
            _levelsCreator = levelsCreator;
            _levelCompletionManagement = levelCompletionManagement;

            _levelCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(_ => CastleHealthView.HideWatchAdvForGetHeartsPanel())
                .AddTo(Disposables);

            _levelCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(_ => CastleHealthView.HideWatchAdvForGetHeartsPanel())
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
            CastleHealthView.ShowWatchAdvForGetHeartsPanel();
        }

        private void OnNoWatchAdvForGetHeartsButtonClicked(Unit unit)
        {
            CastleHealthView.HideWatchAdvForGetHeartsPanel();
        }

        private void OnWatchAdvForGetHeartsButtonClicked(Unit unit)
        {
            CurrentHealth.Value += _addHeartsCountAfterWatchAdv;
            CastleHealthView.HideWatchAdvForGetHeartsPanel();
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
