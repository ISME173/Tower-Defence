using _Project.Scripts.GameoverMagamenet;
using _Project.Scripts.VictoryManagement;
using R3;
using System;
using System.Diagnostics;

using Debug = UnityEngine.Debug;   

namespace _Project.Scripts.LevelsManagement
{

    public class LevelsListController : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();
        private readonly LevelsListView LevelsListView;
        private readonly AddressablesLevelsLoader AddressablesLevelsLoader;
        private readonly LevelsCreator LevelsCreator;

        private VictoryController _victoryController;
        private GameoverController _gameoverController;

        public LevelsListController(LevelsListView levelsListView, AddressablesLevelsLoader addressablesLevelsLoader, LevelsCreator levelsCreator)
        {
            LevelsCreator = levelsCreator;
            LevelsListView = levelsListView;
            AddressablesLevelsLoader = addressablesLevelsLoader;
        }

        public void Initialize(VictoryController victoryController, GameoverController gameoverController)
        {
            _victoryController = victoryController;
            _gameoverController = gameoverController;

            LevelsListView.Initialize(AddressablesLevelsLoader.TotalLevelsCount);

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
            LevelsListView.ShowView();
        }

        private void OnNextLevelsPanelButtonClicked(Unit unit)
        {
            if (LevelsListView.CurrentLevelsPanelIndex + 1 >= LevelsListView.LevelsPanelsCount)
                return;

            LevelsListView.OpenNextLevelsPanel();

            if (LevelsListView.CurrentLevelsPanelIndex + 1 == LevelsListView.LevelsPanelsCount)
                LevelsListView.HideOpenNextLevelsPanelButton();

            LevelsListView.ShowOpenPreviousLevelsPanelButton();
        }

        private void OnPreviousLevelsPanelButtonClicked(Unit unit)
        {
            if (LevelsListView.CurrentLevelsPanelIndex - 1 < 0)
                return;

            LevelsListView.OpenPreviousLevelsPanel();

            if (LevelsListView.CurrentLevelsPanelIndex - 1 < 0)
                LevelsListView.HideOpenPreviousLevelsPanelButton();

            LevelsListView.ShowOpenNextLevelsPanelButton();
        }

        private void OnLevelButtonClicked(int levelIndex)
        {
            LevelsListView.HideView();
            LevelsCreator.CreateLevelByIndex(levelIndex);
        }
    }
}
