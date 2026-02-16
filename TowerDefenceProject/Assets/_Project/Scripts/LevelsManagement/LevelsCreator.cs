using R3;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelsCreator : IDisposable
    {
        private readonly List<LevelObject> AllLevelObjectPrefabs = new List<LevelObject>();
        private readonly Transform CreateLevelPoint;
        private readonly AddressablesLevelsLoader _addressablesLevelsLoader;

        private LevelObject _currentLevelObject;
        private int _currentLevelIndex;

        private readonly ReplaySubject<LevelObject> LevelCreated = new();

        public Observable<LevelObject> ReadOnlyLevelCreated => LevelCreated;
        public LevelObject CurrentLevelObject => _currentLevelObject;

        public LevelsCreator(
            ICollection<LevelObject> levelObjectPrefabs,
            Transform createLevelPoint,
            AddressablesLevelsLoader addressablesLevelsLoader)
        {
            AllLevelObjectPrefabs.AddRange(levelObjectPrefabs);
            CreateLevelPoint = createLevelPoint;
            _addressablesLevelsLoader = addressablesLevelsLoader;

            _currentLevelIndex = 0;

            // 1-й уровень создаётся сразу (не Addressables)
            CreateLevelByIndex(_currentLevelIndex);
        }

        public void Dispose()
        {
            LevelCreated?.OnCompleted();
        }

        public void CreateNextLevel()
        {
            CreateNextLevelAsync().Forget();
        }

        public async UniTask CreateNextLevelAsync()
        {
            int nextLevelIndex = _currentLevelIndex + 1;

            if (nextLevelIndex >= AllLevelObjectPrefabs.Count)
            {
                Debug.LogError($"Invalid level index: {nextLevelIndex}");
                return;
            }

            await CreateLevelByIndexAsync(nextLevelIndex);
        }

        public void RebuildCurrentLevel()
        {
            RebuildCurrentLevelAsync().Forget();
        }

        public async UniTask RebuildCurrentLevelAsync()
        {
            if (_currentLevelObject != null)
                RemoveCurrentLevel();

            await CreateLevelByIndexAsync(_currentLevelIndex);
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

            RemoveCurrentLevel();

            LevelObject levelObject = AllLevelObjectPrefabs[levelIndex];

            LevelObject createdLevelObject = GameObject.Instantiate(levelObject);
            createdLevelObject.transform.position = CreateLevelPoint.position;

            _currentLevelObject = createdLevelObject;
            _currentLevelIndex = levelIndex;

            LevelCreated?.OnNext(createdLevelObject);
        }

        private async UniTask CreateLevelByIndexAsync(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= AllLevelObjectPrefabs.Count)
            {
                Debug.LogError($"Invalid level index: {levelIndex}");
                return;
            }

            RemoveCurrentLevel();

            LevelObject prefabToInstantiate;

            // levelIndex == 0 => уровень #1 (не Addressables)
            if (levelIndex == 0)
            {
                prefabToInstantiate = AllLevelObjectPrefabs[levelIndex];
            }
            else
            {
                if (_addressablesLevelsLoader == null)
                {
                    Debug.LogError("AddressablesLevelsLoader is not bound, but levelIndex > 0 requested.");
                    return;
                }

                try
                {
                    prefabToInstantiate = await _addressablesLevelsLoader.GetPrefabAsync(levelIndex);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load level {levelIndex + 1} from Addressables. {e.Message}");
                    return;
                }
            }

            LevelObject createdLevelObject = GameObject.Instantiate(prefabToInstantiate);
            createdLevelObject.transform.position = CreateLevelPoint.position;

            _currentLevelObject = createdLevelObject;
            _currentLevelIndex = levelIndex;

            LevelCreated?.OnNext(createdLevelObject);
        }
    }
}
