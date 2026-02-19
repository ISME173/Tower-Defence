using _Project.Scripts.GameoverMagamenet;
using _Project.Scripts.VictoryManagement;
using R3;
using System;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelsListController : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();
        private readonly LevelsListView LevelsListView;
        private readonly AddressablesLevelsLoader AddressablesLevelsLoader;
        private readonly LevelsCreator LevelsCreator;
        private readonly LevelsProgressionService LevelsProgressionService;

        private VictoryController _victoryController;
        private GameoverController _gameoverController;

        public LevelsListController(
            LevelsListView levelsListView,
            AddressablesLevelsLoader addressablesLevelsLoader,
            LevelsCreator levelsCreator,
            LevelsProgressionService levelsProgressionService)
        {
            LevelsCreator = levelsCreator;
            LevelsListView = levelsListView;
            AddressablesLevelsLoader = addressablesLevelsLoader;
            LevelsProgressionService = levelsProgressionService;
        }

        public void Initialize(VictoryController victoryController, GameoverController gameoverController)
        {
            _victoryController = victoryController;
            _gameoverController = gameoverController;

            LevelsListView.Initialize(AddressablesLevelsLoader.TotalLevelsCount);
            LevelsListView.UpdateProgress(LevelsProgressionService.IsLevelUnlocked, LevelsProgressionService.GetStars);

            LevelsProgressionService.ReadOnlyProgressChanged
                .Subscribe(_ => LevelsListView.UpdateProgress(LevelsProgressionService.IsLevelUnlocked, LevelsProgressionService.GetStars))
                .AddTo(Disposables);

            LevelsListView.ReadOnlyOnNextLevelsPanelButtonClicked
                .Subscribe(OnNextLevelsPanelButtonClicked)
                .AddTo(Disposables);

            LevelsListView.ReadOnlyOnPreviousLevelsPanelButtonClicked
                .Subscribe(OnPreviousLevelsPanelButtonClicked)
                .AddTo(Disposables);

            LevelsListView.ReadOnlyOnLevelButtonClicked
                .Subscribe(OnLevelButtonClicked)
                .AddTo(Disposables);

            if (LevelsListView.CurrentLevelsPanelIndex + 1 == LevelsListView.LevelsPanelsCount)
                LevelsListView.HideOpenNextLevelsPanelButton();
            if (LevelsListView.CurrentLevelsPanelIndex - 1 < 0)
                LevelsListView.HideOpenPreviousLevelsPanelButton();

            _victoryController.ReadOnlyOnMenuButtonClicked
                .Subscribe(OnMenuButtonClicked)
                .AddTo(Disposables);

            _gameoverController.ReadOnlyOnMenuButtonClicked
                .Subscribe(OnMenuButtonClicked)
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            LevelsListView?.Dispose();
            Disposables.Dispose();
        }

        private void OnMenuButtonClicked(Unit unit)
        {
            LevelsListView.UpdateProgress(LevelsProgressionService.IsLevelUnlocked, LevelsProgressionService.GetStars);
            LevelsListView.ShowView();
        }

        private void OnNextLevelsPanelButtonClicked(Unit unit)
        {
            if (LevelsListView.CurrentLevelsPanelIndex + 1 >= LevelsListView.LevelsPanelsCount)
                return;

            LevelsListView.HideOpenPreviousLevelsPanelButton();
            LevelsListView.HideOpenNextLevelsPanelButton();

            LevelsListView.OpenNextLevelsPanel(() =>
            {
                LevelsListView.ShowOpenPreviousLevelsPanelButton();

                if (LevelsListView.CurrentLevelsPanelIndex + 1 < LevelsListView.LevelsPanelsCount)
                    LevelsListView.ShowOpenNextLevelsPanelButton();
            });
        }

        private void OnPreviousLevelsPanelButtonClicked(Unit unit)
        {
            if (LevelsListView.CurrentLevelsPanelIndex - 1 < 0)
                return;

            LevelsListView.HideOpenPreviousLevelsPanelButton();
            LevelsListView.HideOpenNextLevelsPanelButton();

            LevelsListView.OpenPreviousLevelsPanel(() =>
            {
                LevelsListView.ShowOpenNextLevelsPanelButton();

                if (LevelsListView.CurrentLevelsPanelIndex - 1 >= 0)
                    LevelsListView.ShowOpenPreviousLevelsPanelButton();
            });
        }

        private void OnLevelButtonClicked(int levelIndex)
        {
            if (!LevelsProgressionService.IsLevelUnlocked(levelIndex))
                return;

            LevelsListView.HideView();
            LevelsCreator.CreateLevelByIndex(levelIndex);
        }
    }
}
