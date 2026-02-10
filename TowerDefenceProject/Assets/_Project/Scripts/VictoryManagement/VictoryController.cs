using _Project.Scripts.LevelsManagement;
using R3;
using System;
using UnityEngine;

namespace _Project.Scripts.VictoryManagement
{
    public class VictoryController : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();

        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelsCompletionManagement;

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
            Disposables.Dispose();
        }

        private void OnLevelCompleted(Unit unit)
        {
            _levelsCreator.CreateNextLevel();
        }
    }
}
