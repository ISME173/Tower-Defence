using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelsCreator : IDisposable
    {
        private readonly List<LevelObject> AllLevelObjectPrefabs = new List<LevelObject>();
        private readonly Transform CreateLevelPoint;

        private LevelObject _currentLevelObject;
        private int _currentLevelIndex;

        private readonly ReplaySubject<LevelObject> LevelCreated = new();

        public Observable<LevelObject> ReadOnlyLevelCreated => LevelCreated;
        public LevelObject CurrentLevelObject => _currentLevelObject;

        public LevelsCreator(ICollection<LevelObject> levelObjectPrefabs, Transform createLevelPoint)
        {
            AllLevelObjectPrefabs.AddRange(levelObjectPrefabs);
            CreateLevelPoint = createLevelPoint;

            _currentLevelIndex = 0;

            CreateLevelByIndex(_currentLevelIndex);
        }

        public void Dispose()
        {
           LevelCreated?.OnCompleted();
        }

        public void CreateNextLevel()
        {
            int nextLevelIndex = _currentLevelIndex + 1;

            if (nextLevelIndex >= AllLevelObjectPrefabs.Count)
            {
                Debug.LogError($"Invalid level index: {nextLevelIndex}");
                return;
            }

            CreateLevelByIndex(nextLevelIndex);
        }

        public void RebuildCurrentLevel()
        {
            if (_currentLevelObject != null)
                RemoveCurrentLevel();
             
            CreateLevelByIndex(_currentLevelIndex);
        }

        private void RemoveCurrentLevel()
        {
            if (_currentLevelObject == null)
                return;

            GameObject.Destroy(_currentLevelObject.gameObject);
        }

        private void CreateLevelByIndex(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= AllLevelObjectPrefabs.Count)
            {
                Debug.LogError($"Invalid level index: {levelIndex}");
                return;
            }

            // Clear previous level
            RemoveCurrentLevel();

            LevelObject levelObject = AllLevelObjectPrefabs[levelIndex];

            LevelObject createdLevelObject = GameObject.Instantiate(levelObject);
            createdLevelObject.transform.position = CreateLevelPoint.position;

            _currentLevelObject = createdLevelObject;

            _currentLevelIndex = levelIndex;
            LevelCreated?.OnNext(createdLevelObject);
        }
    }
}
