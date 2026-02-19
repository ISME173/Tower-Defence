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

        public Observable<LevelObject> ReadOnlyLevelCreated => LevelCreated;
        public LevelObject CurrentLevelObject => _currentLevelObject;
        public int CurrentLevelIndex => _currentLevelIndex;

        public LevelsCreator(
            ICollection<LevelObject> levelObjectPrefabs,
            Transform createLevelPoint,
            AddressablesLevelsLoader addressablesLevelsLoader)
        {
            AllLevelObjectPrefabs.AddRange(levelObjectPrefabs);
            CreateLevelPoint = createLevelPoint;
            _addressablesLevelsLoader = addressablesLevelsLoader;

            if (_addressablesLevelsLoader != null)
            {
                _addressablesLevelsLoader.ReadOnlyLevelPrefabLoaded
                    .Subscribe(x => OnAddressablesLevelPrefabLoaded(x.LevelIndex, x.Prefab))
                    .AddTo(_disposables);
            }

            _currentLevelIndex = 0;
        }

        public void Initialize(CameraMoving cameraMoving)
        {
            _cameraMoving = cameraMoving;

            // 1-й уровень создаётся сразу (не Addressables)
            CreateLevelByIndex(_currentLevelIndex);
        }

        public void Dispose()
        {
            _disposables.Dispose();
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

        public void CreateLevelByIndex(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= AllLevelObjectPrefabs.Count)
            {
                Debug.LogError($"Invalid level index: {levelIndex}");
                return;
            }

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

            // Если в списке уже лежит загруженный addressables-префаб — создаём сразу
            if (AllLevelObjectPrefabs[levelIndex] != null)
            {
                CreateLevelByIndexSync(levelIndex, AllLevelObjectPrefabs[levelIndex]);
                return;
            }

            // Иначе — ждём загрузку и создаём после ожидания.
            CreateLevelByIndexAsync(levelIndex).Forget();
        }

        private void OnAddressablesLevelPrefabLoaded(int levelIndex, LevelObject prefab)
        {
            if (prefab == null)
                return;

            // гарантируем размер списка (если в инспекторе не было элементов под 2+)
            while (AllLevelObjectPrefabs.Count <= levelIndex)
                AllLevelObjectPrefabs.Add(null);

            AllLevelObjectPrefabs[levelIndex] = prefab;

            Debug.Log($"[LevelsCreator] Cached Addressables level prefab for level #{levelIndex + 1} into AllLevelObjectPrefabs[{levelIndex}]");
        }

        private void CreateLevelByIndexSync(int levelIndex, LevelObject prefabToInstantiate)
        {
            RemoveCurrentLevel();

            LevelObject createdLevelObject = GameObject.Instantiate(prefabToInstantiate);
            createdLevelObject.transform.position = CreateLevelPoint.position;

            _currentLevelObject = createdLevelObject;
            _currentLevelIndex = levelIndex;

            LevelCreated?.OnNext(createdLevelObject);
            _cameraMoving.UnlockMoving();
        }

        private void RemoveCurrentLevel()
        {
            if (_currentLevelObject == null)
                return;

            GameObject.Destroy(_currentLevelObject.gameObject);
        }

        private async UniTask CreateLevelByIndexAsync(int levelIndex)
        {
            // Если пока ждали ивентом уже закэшировали — создаём сразу
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

            // закэшируем на всякий случай (если событие ещё не пришло)
            OnAddressablesLevelPrefabLoaded(levelIndex, prefabToInstantiate);

            CreateLevelByIndexSync(levelIndex, prefabToInstantiate);
        }
    }
}
