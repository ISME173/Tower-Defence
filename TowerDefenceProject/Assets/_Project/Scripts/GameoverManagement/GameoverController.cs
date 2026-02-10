using _Project.Scripts.LevelsManagement;
using R3;
using System;
using UnityEngine;

namespace _Project.Scripts.GameoverMagamenet
{

    public class GameoverController : IDisposable
    {
        private readonly CompositeDisposable Disposables = new(); 

        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelCompletionManagement;

        public void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement)
        {
            _levelsCreator = levelsCreator;
            _levelCompletionManagement = levelCompletionManagement;

            _levelCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(OnLevelFailed)
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }

        private void OnLevelFailed(Unit unit)
        {
            _levelsCreator.RebuildCurrentLevel();
        }
    }
}
