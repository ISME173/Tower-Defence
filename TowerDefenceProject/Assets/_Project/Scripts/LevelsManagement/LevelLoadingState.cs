namespace _Project.Scripts.LevelsManagement
{
    public enum LevelLoadingStatus
    {
        Idle = 0,
        Loading = 1,
        Loaded = 2,
        Failed = 3
    }

    public readonly struct LevelLoadingState
    {
        public readonly int LevelIndex;
        public readonly LevelLoadingStatus Status;
        public readonly float Progress;
        public readonly string Error;

        public LevelLoadingState(int levelIndex, LevelLoadingStatus status, float progress = 0f, string error = null)
        {
            LevelIndex = levelIndex;
            Status = status;
            Progress = progress;
            Error = error;
        }
    }
}