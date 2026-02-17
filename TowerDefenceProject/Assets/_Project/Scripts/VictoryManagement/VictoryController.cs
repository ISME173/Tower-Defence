using _Project.Scripts.LevelsManagement;
using R3;
using System;

namespace _Project.Scripts.VictoryManagement
{
    public class VictoryController : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();
        private readonly VictoryView VictoryView;

        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelsCompletionManagement;

        public Observable<Unit> ReadOnlyOnMenuButtonClicked => VictoryView.ReadOnlyOnMenuButtonClicked;

        public VictoryController(VictoryView victoryView)
        {
            VictoryView = victoryView;

            VictoryView.ReadOnlyOnContinueButtonClicked
                .Subscribe(OnContinueButtonDown)
                .AddTo(Disposables);

            VictoryView.ReadOnlyOnRestartButtonClicked
                .Subscribe(OnRestartButtonDown)
                .AddTo(Disposables);

            VictoryView.Initialize();
        }

        public void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement levelsCompletionManagement)
        {
            _levelsCreator = levelsCreator;
            _levelsCompletionManagement = levelsCompletionManagement;

            _levelsCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(OnLevelCompleted)
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            VictoryView?.Dispose();
            Disposables.Dispose();
        }

        private void OnLevelCompleted(Unit unit)
        {
            VictoryView.Show();
        }

        private void OnContinueButtonDown(Unit unit)
        {
            VictoryView.Hide();
            _levelsCreator.CreateNextLevel();
        }

        private void OnRestartButtonDown(Unit unit)
        {
            VictoryView.Hide();
            _levelsCreator.RebuildCurrentLevel();
        }
    }
}
