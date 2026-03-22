using _Project.Scripts.Advertisement;
using _Project.Scripts.Audio;
using _Project.Scripts.CameraControll;
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
        private readonly MoneyView MoneyView;
        private readonly CompositeDisposable Disposables = new();
        private readonly ReactiveProperty<int> CurrentAmountOfMoney = new();

        private int _getMoneysCountAfterWatchAdv;
        private LevelCompletionManagement _levelCompletionManagement;
        private EnemiesSpawner _enemiesSpawner;
        private LevelsCreator _levelCreator;
        private CameraMoving _cameraMoving;

        private IAudioService _audioService;
        private AudioEvent _buttonClickAudioEvent;

        private IAdvertisement _advertisement;

        private readonly Subject<Unit> OnMoneysAdded = new Subject<Unit>();

        public Observable<Unit> ReadOnlyOnMoneysAdded => OnMoneysAdded;
        public ReadOnlyReactiveProperty<int> ReadOnlyCurrentAmountOfMoney => CurrentAmountOfMoney;

        public MoneyManagement(MoneyView moneyView)
        {
            MoneyView = moneyView;

            MoneyView.ReadOnlyOnGetMoneyButtonClicked
                .Subscribe(OnGetMoneyButtonClicked)
                .AddTo(Disposables);

            MoneyView.ReadOnlyOnNoWatchAdvButtonClicked
                .Subscribe(OnNoWatchAdvForGetMoneyButtonClicked)
                .AddTo(Disposables);

            MoneyView.ReadOnlyOnWatchAdvButtonClicked
                .Subscribe(OnWatchAdvForGetMoneyButtonClicked)
                .AddTo(Disposables);
        }

        public void Initialze(EnemiesSpawner enemiesSpawner, LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement, CameraMoving cameraMoving, 
            IAdvertisement advertisement, int getMoneysCountAfterWatchAdv, IAudioService audioService, AudioEvent buttonClickAudioEvent)
        {
            _enemiesSpawner = enemiesSpawner;
            _levelCreator = levelsCreator;
            _getMoneysCountAfterWatchAdv = getMoneysCountAfterWatchAdv;
            _levelCompletionManagement = levelCompletionManagement;
            _advertisement = advertisement;
            _cameraMoving = cameraMoving;
            _audioService = audioService;
            _buttonClickAudioEvent = buttonClickAudioEvent;

            _enemiesSpawner.ReadOnlyEnemyDied
                .Subscribe(OnEnemyDied)
                .AddTo(Disposables);

            _levelCreator.ReadOnlyLevelCreated
                .Subscribe(OnLevelCreated)
                .AddTo(Disposables);

            _levelCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(_ =>
                {
                    MoneyView.HideMainView();
                    MoneyView.HideWatchAdvForGetMoneyPanel();
                })
                .AddTo(Disposables);

            _levelCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(_ =>
                {
                    MoneyView.HideMainView();
                    MoneyView.HideWatchAdvForGetMoneyPanel();
                })
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables.Dispose();
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

        private void OnGetMoneyButtonClicked(Unit unit)
        {
            _cameraMoving.LockMoving();
            Time.timeScale = 0;

            _audioService.PlayOneShot(_buttonClickAudioEvent);
            MoneyView.ShowWatchAdvForGetMoneyPanel();
        }

        private void OnWatchAdvForGetMoneyButtonClicked(Unit unit)
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);

            _advertisement.ShowRewardedAdv(() =>
            {
                CurrentAmountOfMoney.Value += _getMoneysCountAfterWatchAdv;

                MoneyView.HideWatchAdvForGetMoneyPanel();

                _cameraMoving.UnlockMoving();
                Time.timeScale = 1;
            });
        }

        private void OnNoWatchAdvForGetMoneyButtonClicked(Unit unit)
        {
            _cameraMoving.UnlockMoving();
            Time.timeScale = 1;

            _audioService.PlayOneShot(_buttonClickAudioEvent);
            MoneyView.HideWatchAdvForGetMoneyPanel();
        }

        private void OnLevelCreated(LevelObject levelObject)
        {
            CurrentAmountOfMoney.Value = levelObject.InitialAmountOfMoney;
            MoneyView.ShowMainView();
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
