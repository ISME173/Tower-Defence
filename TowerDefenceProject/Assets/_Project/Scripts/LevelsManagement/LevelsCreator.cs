using R3;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Project.Scripts.CameraControll;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelsCreator : IDisposable
    {
        private readonly List<LevelObject> AllLevelObjectPrefabs = new List<LevelObject>();
        private readonly Transform CreateLevelPoint;
        private readonly AddressablesLevelsLoader _addressablesLevelsLoader;

        private readonly CompositeDisposable _disposables = new();

        private CameraMoving _cameraMoving;
        private LevelObject _currentLevelObject;
        private int _currentLevelIndex;

        private readonly ReplaySubject<LevelObject> LevelCreated = new();
        private readonly Subject<LevelObject> LevelDestroyStart = new();

        public Observable<LevelObject> ReadOnlyLevelCreated => LevelCreated;
        public Observable<LevelObject> ReadOnlyLevelDestroyStart => LevelDestroyStart;
        public LevelObject CurrentLevelObject => _currentLevelObject;
        public int CurrentLevelIndex => _currentLevelIndex;

        public LevelsCreator(
            LevelObject firstLevelOnScene,
            LevelObject firstLevelPrefab,
            Transform createLevelPoint,
            AddressablesLevelsLoader addressablesLevelsLoader)
        {
            AllLevelObjectPrefabs.Add(firstLevelPrefab);
            CreateLevelPoint = createLevelPoint;
            _addressablesLevelsLoader = addressablesLevelsLoader;

            if (_addressablesLevelsLoader != null)
            {
                _addressablesLevelsLoader.ReadOnlyLevelPrefabLoaded
                    .Subscribe(x => OnAddressablesLevelPrefabLoaded(x.LevelIndex, x.Prefab))
                    .AddTo(_disposables);
            }

            _currentLevelObject = firstLevelOnScene;
            _currentLevelIndex = 0;
        }

        public void Initialize(CameraMoving cameraMoving)
        {
            Initialize(cameraMoving, 0);
        }

        public void Initialize(CameraMoving cameraMoving, int startLevelIndex)
        {
            _cameraMoving = cameraMoving ?? throw new ArgumentNullException(nameof(cameraMoving));

            if (_currentLevelObject != null)
                _currentLevelIndex = Math.Max(0, _currentLevelObject.LevelNumber - 1);
            else
                _currentLevelIndex = 0;

            int normalizedStartLevelIndex = Mathf.Clamp(startLevelIndex, 0, Math.Max(0, GetTotalLevelsCount() - 1));

            if (normalizedStartLevelIndex == _currentLevelIndex)
            {
                if (_currentLevelObject == null)
                {
                    CreateLevelByIndex(normalizedStartLevelIndex);
                    return;
                }

                LevelCreated?.OnNext(_currentLevelObject);
                _cameraMoving.UnlockMoving();
                return;
            }

            RemoveCurrentLevel();
            CreateLevelByIndex(normalizedStartLevelIndex);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            LevelCreated?.OnCompleted();
        }

        public void CreateNextLevel()
        {
            int nextLevelIndex = _currentLevelIndex + 1;

            if (nextLevelIndex >= GetTotalLevelsCount())
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

        public void CreateLevelByIndex(int levelIndex)
        {
            int totalLevelsCount = GetTotalLevelsCount();

            if (levelIndex < 0 || levelIndex >= totalLevelsCount)
            {
                Debug.LogError($"Invalid level index: {levelIndex}");
                return;
            }

            EnsurePrefabsCapacity(levelIndex);

            if (AllLevelObjectPrefabs[levelIndex] != null)
            {
                CreateLevelByIndexSync(levelIndex, AllLevelObjectPrefabs[levelIndex]);
                return;
            }

            if (levelIndex == 0)
            {
                Debug.LogError("First level prefab is not set.");
                return;
            }

            if (_addressablesLevelsLoader == null)
            {
                Debug.LogError("AddressablesLevelsLoader is not bound, but levelIndex > 0 requested.");
                return;
            }

            CreateLevelByIndexAsync(levelIndex).Forget();
        }

        private void OnAddressablesLevelPrefabLoaded(int levelIndex, LevelObject prefab)
        {
            if (prefab == null)
                return;

            EnsurePrefabsCapacity(levelIndex);
            AllLevelObjectPrefabs[levelIndex] = prefab;

            Debug.Log($"[LevelsCreator] Cached Addressables level prefab for level #{levelIndex + 1} into AllLevelObjectPrefabs[{levelIndex}]");
        }

        private void CreateLevelByIndexSync(int levelIndex, LevelObject prefabToInstantiate)
        {
            RemoveCurrentLevel();

            if (prefabToInstantiate == null)
                throw new NullReferenceException($"{nameof(prefabToInstantiate)} is empty!");

            LevelObject createdLevelObject = GameObject.Instantiate(prefabToInstantiate);
            createdLevelObject.transform.position = CreateLevelPoint.position;

            _currentLevelObject = createdLevelObject;
            _currentLevelIndex = levelIndex;

            LevelCreated?.OnNext(_currentLevelObject);
            _cameraMoving.UnlockMoving();
        }

        private void RemoveCurrentLevel()
        {
            if (_currentLevelObject == null)
                return;

            LevelDestroyStart?.OnNext(_currentLevelObject);

            GameObject.Destroy(_currentLevelObject.gameObject);
            _currentLevelObject = null;
        }

        private async UniTask CreateLevelByIndexAsync(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < AllLevelObjectPrefabs.Count && AllLevelObjectPrefabs[levelIndex] != null)
            {
                CreateLevelByIndexSync(levelIndex, AllLevelObjectPrefabs[levelIndex]);
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

            OnAddressablesLevelPrefabLoaded(levelIndex, prefabToInstantiate);
            CreateLevelByIndexSync(levelIndex, prefabToInstantiate);
        }

        private int GetTotalLevelsCount()
        {
            int addressablesLevelsCount = _addressablesLevelsLoader?.TotalLevelsCount ?? 0;
            return Math.Max(AllLevelObjectPrefabs.Count, addressablesLevelsCount);
        }

        private void EnsurePrefabsCapacity(int levelIndex)
        {
            while (AllLevelObjectPrefabs.Count <= levelIndex)
                AllLevelObjectPrefabs.Add(null);
        }
    }
}
