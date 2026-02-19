using _Project.Scripts.Saves;
using R3;
using System;

namespace _Project.Scripts.LevelsManagement
{
    public sealed class LevelsProgressionService : IDisposable
    {
        private const string MaxUnlockedLevelIndexKey = "levels.maxUnlockedLevelIndex";
        private const string StarsByLevelIndexKey = "levels.starsByLevelIndex";

        private readonly Subject<Unit> ProgressChanged = new();
        public Observable<Unit> ReadOnlyProgressChanged => ProgressChanged;

        private ISaves _saves;
        private int _totalLevelsCount;

        private int _maxUnlockedLevelIndex;
        private int[] _starsByLevelIndex = Array.Empty<int>();

        public void Initialize(ISaves saves, int totalLevelsCount)
        {
            _saves = saves ?? throw new ArgumentNullException(nameof(saves));
            _totalLevelsCount = Math.Max(0, totalLevelsCount);

            _maxUnlockedLevelIndex = _saves.GetInt(MaxUnlockedLevelIndexKey, 0);

            if (_totalLevelsCount > 0)
                _maxUnlockedLevelIndex = Math.Clamp(_maxUnlockedLevelIndex, 0, _totalLevelsCount - 1);
            else
                _maxUnlockedLevelIndex = 0;

            var stars = _saves.GetObject(StarsByLevelIndexKey, new int[_totalLevelsCount]) ?? new int[_totalLevelsCount];

            if (stars.Length != _totalLevelsCount)
            {
                var resized = new int[_totalLevelsCount];
                Array.Copy(stars, resized, Math.Min(stars.Length, resized.Length));
                stars = resized;
            }

            for (int i = 0; i < stars.Length; i++)
                stars[i] = Math.Clamp(stars[i], 0, 3);

            _starsByLevelIndex = stars;

            // гарантируем, что хотя бы 1 уровень открыт
            if (_totalLevelsCount > 0)
                _maxUnlockedLevelIndex = Math.Max(_maxUnlockedLevelIndex, 0);

            Save();
            ProgressChanged.OnNext(Unit.Default);
        }

        public bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex < 0)
                return false;

            return levelIndex <= _maxUnlockedLevelIndex;
        }

        public int GetStars(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _starsByLevelIndex.Length)
                return 0;

            return _starsByLevelIndex[levelIndex];
        }

        public void ApplyLevelCompleted(int levelIndex, int starsEarned)
        {
            if (_saves == null)
                throw new InvalidOperationException($"{nameof(LevelsProgressionService)} is not initialized.");

            if (_totalLevelsCount <= 0)
                return;

            if (levelIndex < 0 || levelIndex >= _totalLevelsCount)
                return;

            starsEarned = Math.Clamp(starsEarned, 1, 3);

            bool changed = false;

            int current = _starsByLevelIndex[levelIndex];
            int newStars = Math.Max(current, starsEarned);
            if (newStars != current)
            {
                _starsByLevelIndex[levelIndex] = newStars;
                changed = true;
            }

            int newMaxUnlocked = _maxUnlockedLevelIndex;
            int nextLevelIndex = levelIndex + 1;
            if (nextLevelIndex < _totalLevelsCount)
                newMaxUnlocked = Math.Max(newMaxUnlocked, nextLevelIndex);

            if (newMaxUnlocked != _maxUnlockedLevelIndex)
            {
                _maxUnlockedLevelIndex = newMaxUnlocked;
                changed = true;
            }

            if (!changed)
                return;

            Save();
            ProgressChanged.OnNext(Unit.Default);
        }

        private void Save()
        {
            _saves.SetInt(MaxUnlockedLevelIndexKey, _maxUnlockedLevelIndex);
            _saves.SetObject(StarsByLevelIndexKey, _starsByLevelIndex, prettyPrint: false);
            _saves.Save();
        }

        public void Dispose()
        {
            ProgressChanged?.Dispose();
        }
    }
}