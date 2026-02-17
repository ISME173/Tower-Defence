using _Project.Scripts.LevelsManagement;
using R3;
using System;

namespace _Project.Scripts.GameoverMagamenet
{
    public class GameoverController : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();
        private readonly GameoverView GameoverView;

        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelCompletionManagement;

        public Observable<Unit> ReadOnlyOnMenuButtonClicked => GameoverView.ReadOnlyOnMenuButtonClicked;

        public GameoverController(GameoverView gameoverView)
        {
            GameoverView = gameoverView;
        }

        public void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement)
        {
            _levelsCreator = levelsCreator;
            _levelCompletionManagement = levelCompletionManagement;

            GameoverView.Initialize();

            _levelCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(OnLevelFailed)
                .AddTo(Disposables);

            GameoverView.ReadOnlyOnRestartButtonClicked
                .Subscribe(OnRestartButtonClicked)
                .AddTo(Disposables);

            GameoverView.ReadOnlyOnMenuButtonClicked
                .Subscribe(_ => GameoverView.Hide())
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            GameoverView?.Dispose();
            Disposables.Dispose();
        }

        private void OnRestartButtonClicked(Unit unit)
        {
            GameoverView.Hide();
            _levelsCreator.RebuildCurrentLevel();
        }

        private void OnLevelFailed(Unit unit)
        {
            GameoverView.Show();
        }
    }
}
