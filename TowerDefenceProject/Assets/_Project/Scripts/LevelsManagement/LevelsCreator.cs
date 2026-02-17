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

            // Пересоздание текущего уровня должно происходить сразу
            CreateLevelByIndex(_currentLevelIndex);
        }

        public void CreateLevelByIndex(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= AllLevelObjectPrefabs.Count)
            {
                Debug.LogError($"Invalid level index: {levelIndex}");
                return;
            }

            // levelIndex == 0 => уровень #1 (не Addressables), создаём синхронно
            if (levelIndex == 0)
            {
                CreateLevelByIndexSync(levelIndex, AllLevelObjectPrefabs[levelIndex]);
                return;
            }

            if (_addressablesLevelsLoader == null)
            {
                Debug.LogError("AddressablesLevelsLoader is not bound, but levelIndex > 0 requested.");
                return;
            }

            // Если уже загружен — создаём сразу.
            if (_addressablesLevelsLoader.TryGetLoadedPrefab(levelIndex, out var loadedPrefab) && loadedPrefab != null)
            {
                CreateLevelByIndexSync(levelIndex, loadedPrefab);
                return;
            }

            // Иначе — ждём загрузку и создаём после ожидания.
            CreateLevelByIndexAsync(levelIndex).Forget();
        }

        private void CreateLevelByIndexSync(int levelIndex, LevelObject prefabToInstantiate)
        {
            RemoveCurrentLevel();

            LevelObject createdLevelObject = GameObject.Instantiate(prefabToInstantiate);
            createdLevelObject.transform.position = CreateLevelPoint.position;

            _currentLevelObject = createdLevelObject;
            _currentLevelIndex = levelIndex;

            LevelCreated?.OnNext(createdLevelObject);
        }

        private void RemoveCurrentLevel()
        {
            if (_currentLevelObject == null)
                return;

            GameObject.Destroy(_currentLevelObject.gameObject);
        }

        private async UniTask CreateLevelByIndexAsync(int levelIndex)
        {
            // Повторная быстрая проверка на случай, если пока ждали — он уже загрузился
            if (_addressablesLevelsLoader.TryGetLoadedPrefab(levelIndex, out var loadedPrefab) && loadedPrefab != null)
            {
                CreateLevelByIndexSync(levelIndex, loadedPrefab);
                return;
            }

            LevelObject prefabToInstantiate;

            try
            {
                prefabToInstantiate = await _addressablesLevelsLoader.GetPrefabAsync(levelIndex);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load level {levelIndex + 1} from Addressables. {e.Message}");
                return;
            }

            CreateLevelByIndexSync(levelIndex, prefabToInstantiate);
        }
    }
}
