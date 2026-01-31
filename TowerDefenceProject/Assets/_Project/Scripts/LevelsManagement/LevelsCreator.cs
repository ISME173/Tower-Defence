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

        private int _currentLevelIndex;

        public readonly ReplaySubject<LevelObject> LevelCreated = new();

        public LevelObject CurrentLevelObject { get; private set; } = null;

        public LevelsCreator(ICollection<LevelObject> levelObjectPrefabs, Transform createLevelPoint)
        {
            AllLevelObjectPrefabs.AddRange(levelObjectPrefabs);
            CreateLevelPoint = createLevelPoint;

            _currentLevelIndex = 0;

            CreateLevelByIndex(_currentLevelIndex);
        }

        private void CreateLevelByIndex(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= AllLevelObjectPrefabs.Count)
            {
                Debug.LogError($"Invalid level index: {levelIndex}");
                return;
            }

            LevelObject levelObject = AllLevelObjectPrefabs[levelIndex];

            LevelObject createdLevelObject = GameObject.Instantiate(levelObject);
            createdLevelObject.transform.SetParent(CreateLevelPoint);
            createdLevelObject.transform.localPosition = Vector3.zero;

            CurrentLevelObject = levelObject;

            _currentLevelIndex = levelIndex;
            LevelCreated?.OnNext(createdLevelObject);
        }

        public void Dispose()
        {
            LevelCreated?.OnCompleted();
        }
    }
}
