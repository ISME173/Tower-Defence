using _Project.Scripts.Castle;
using R3;
using System;

namespace _Project.Scripts.LevelsManagement
{
    public sealed class LevelsProgressionRuntimeTracker : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();

        private LevelsProgressionService _levelsProgressionService;
        private LevelsCreator _levelsCreator;
        private CastleHealthManagement _castleHealthManagement;
        private LevelCompletionManagement _levelCompletionManagement;

        public void Initialize(
            LevelsProgressionService levelsProgressionService,
            LevelsCreator levelsCreator,
            CastleHealthManagement castleHealthManagement,
            LevelCompletionManagement levelCompletionManagement)
        {
            _levelsProgressionService = levelsProgressionService;
            _levelsCreator = levelsCreator;
            _castleHealthManagement = castleHealthManagement;
            _levelCompletionManagement = levelCompletionManagement;

            _levelCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(OnLevelCompleted)
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }

        private void OnLevelCompleted(Unit unit)
        {
            var levelObject = _levelsCreator.CurrentLevelObject;
            if (levelObject == null)
                return;

            int remainingHealth = _castleHealthManagement.ReadOnlyCurrentHealth.CurrentValue;
            int stars = levelObject.CalculateStarsByRemainingHealth(remainingHealth);

            _levelsProgressionService.ApplyLevelCompleted(_levelsCreator.CurrentLevelIndex, stars);
        }
    }
}