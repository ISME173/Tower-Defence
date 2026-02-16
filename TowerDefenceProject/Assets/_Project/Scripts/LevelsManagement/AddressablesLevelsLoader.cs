using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Project.Scripts.LevelsManagement
{
    public sealed class AddressablesLevelsLoader : IDisposable
    {
        private readonly Dictionary<int, string> _levelIndexToKey = new();
        private readonly Dictionary<int, AsyncOperationHandle<GameObject>> _handlesByLevelIndex = new();

        private readonly ReactiveProperty<LevelLoadingState> _levelLoadingState = new(new LevelLoadingState(0, LevelLoadingStatus.Idle));
        public ReadOnlyReactiveProperty<LevelLoadingState> ReadOnlyLevelLoadingState => _levelLoadingState;

        public AddressablesLevelsLoader(IReadOnlyList<string> levelAddressKeysStartingFromLevel2)
        {
            if (levelAddressKeysStartingFromLevel2 == null)
                return;

            // levelIndex: 1 => уровень #2, 2 => уровень #3, ...
            for (int i = 0; i < levelAddressKeysStartingFromLevel2.Count; i++)
                _levelIndexToKey[i + 1] = levelAddressKeysStartingFromLevel2[i];
        }

        public void Dispose()
        {
            foreach (var kv in _handlesByLevelIndex)
            {
                var handle = kv.Value;
                if (handle.IsValid())
                    Addressables.Release(handle);
            }

            _handlesByLevelIndex.Clear();
        }

        public bool HasKeyForLevelIndex(int levelIndex) => _levelIndexToKey.ContainsKey(levelIndex);

        public bool IsLoaded(int levelIndex)
        {
            return _handlesByLevelIndex.TryGetValue(levelIndex, out var handle)
                   && handle.IsValid()
                   && handle.Status == AsyncOperationStatus.Succeeded
                   && handle.Result != null;
        }

        public void WarmupAll()
        {
            foreach (var levelIndex in _levelIndexToKey.Keys)
                EnsureLoadedAsync(levelIndex).Forget();
        }

        public async UniTask EnsureLoadedAsync(int levelIndex)
        {
            if (IsLoaded(levelIndex))
            {
                _levelLoadingState.Value = new LevelLoadingState(levelIndex, LevelLoadingStatus.Loaded, 1f);
                return;
            }

            if (!_levelIndexToKey.TryGetValue(levelIndex, out var key) || string.IsNullOrWhiteSpace(key))
                throw new ArgumentOutOfRangeException(nameof(levelIndex), $"No Addressables key for level index: {levelIndex}");

            Debug.Log($"[AddressablesLevelsLoader] Start loading level #{levelIndex + 1} (key='{key}')");

            if (_handlesByLevelIndex.TryGetValue(levelIndex, out var existing))
            {
                _levelLoadingState.Value = new LevelLoadingState(levelIndex, LevelLoadingStatus.Loading, existing.PercentComplete);

                float lastLoggedProgress = -1f;
                while (!existing.IsDone)
                {
                    float p = existing.PercentComplete;
                    _levelLoadingState.Value = new LevelLoadingState(levelIndex, LevelLoadingStatus.Loading, p);

                    if (p >= 0f && p <= 1f && p - lastLoggedProgress >= 0.25f)
                    {
                        lastLoggedProgress = p;
                        Debug.Log($"[AddressablesLevelsLoader] Loading level #{levelIndex + 1} (key='{key}') progress={(p * 100f):0}%");
                    }

                    await UniTask.Yield();
                }

                if (existing.Status != AsyncOperationStatus.Succeeded || existing.Result == null)
                {
                    string msg = $"Failed to load Addressables level prefab by key: {key}";
                    _levelLoadingState.Value = new LevelLoadingState(levelIndex, LevelLoadingStatus.Failed, existing.PercentComplete, msg);
                    Debug.LogError($"[AddressablesLevelsLoader] Level #{levelIndex + 1} (key='{key}') load FAILED. {msg}");
                    throw new InvalidOperationException(msg);
                }

                _levelLoadingState.Value = new LevelLoadingState(levelIndex, LevelLoadingStatus.Loaded, 1f);
                Debug.Log($"[AddressablesLevelsLoader] Level #{levelIndex + 1} (key='{key}') loaded OK");
                return;
            }

            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            _handlesByLevelIndex[levelIndex] = handle;

            _levelLoadingState.Value = new LevelLoadingState(levelIndex, LevelLoadingStatus.Loading, handle.PercentComplete);

            float lastLoggedProgressNew = -1f;
            while (!handle.IsDone)
            {
                float p = handle.PercentComplete;
                _levelLoadingState.Value = new LevelLoadingState(levelIndex, LevelLoadingStatus.Loading, p);

                if (p >= 0f && p <= 1f && p - lastLoggedProgressNew >= 0.25f)
                {
                    lastLoggedProgressNew = p;
                    Debug.Log($"[AddressablesLevelsLoader] Loading level #{levelIndex + 1} (key='{key}') progress={(p * 100f):0}%");
                }

                await UniTask.Yield();
            }

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                string msg = $"Failed to load Addressables level prefab by key: {key}";
                _levelLoadingState.Value = new LevelLoadingState(levelIndex, LevelLoadingStatus.Failed, handle.PercentComplete, msg);
                Debug.LogError($"[AddressablesLevelsLoader] Level #{levelIndex + 1} (key='{key}') load FAILED. {msg}");
                throw new InvalidOperationException(msg);   
            }

            _levelLoadingState.Value = new LevelLoadingState(levelIndex, LevelLoadingStatus.Loaded, 1f);
            Debug.Log($"[AddressablesLevelsLoader] Level #{levelIndex + 1} (key='{key}') loaded OK");
        }

        public async UniTask<LevelObject> GetPrefabAsync(int levelIndex)
        {
            await EnsureLoadedAsync(levelIndex);

            var handle = _handlesByLevelIndex[levelIndex];
            if (!handle.IsValid() || handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                throw new InvalidOperationException($"Addressables handle invalid for level index: {levelIndex}");

            if (!handle.Result.TryGetComponent<LevelObject>(out var levelObject) || levelObject == null)
                throw new InvalidOperationException($"Loaded prefab by key '{_levelIndexToKey[levelIndex]}' does not contain component '{nameof(LevelObject)}'.");

            return levelObject;
        }

        public bool TryGetLoadedPrefab(int levelIndex, out LevelObject prefab)
        {
            prefab = null;

            if (!IsLoaded(levelIndex))
                return false;

            var go = _handlesByLevelIndex[levelIndex].Result;
            if (go == null)
                return false;

            return go.TryGetComponent(out prefab) && prefab != null;
        }
    }
}