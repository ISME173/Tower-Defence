using _Project.Scripts.LevelsManagement;
using _Project.Scripts.Utilities;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace _Project.Scripts.EnemiesManagement.Spawn
{
    public class EnemiesSpawner : IDisposable
    {
        private readonly Dictionary<Enemy, CompositeDisposable> EnemiesInLevel = new();
        private readonly Dictionary<string, ObjectPoolWithQueue<Enemy>> ObjectPoolsByEnemy = new();
        private readonly CompositeDisposable Disposables = new();

        private Transform _enemiesContainer;
        private Transform _currentSpawnEnemiesPoint;

        private LevelCompletionManagement _levelCompletionManagement;
        private LevelsCreator _levelsCreator;

        private List<Transform> _currentMovingPoints;
        private EnemysInLevelSpawnSeqence _currentEnemiesInLevelSpawnSequence;
        private CancellationTokenSource _spawnCancellationTokenSource;
        private bool _spawningProcessActive = false;

        // R3 events
        private readonly Subject<Enemy> EnemyMovedToLastPoint = new(), EnemyDied = new();
        private readonly Subject<Unit> AllEnemiesDefeated = new();

        public Observable<Enemy> ReadOnlyEnemyDied => EnemyDied;
        public Observable<Enemy> ReadOnlyEnemyMovedToLastPoint => EnemyMovedToLastPoint;
        public Observable<Unit> ReadOnlyAllEnemiedDefeated => AllEnemiesDefeated;

        public void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement, Transform enemiesContainer)
        {
            _enemiesContainer = enemiesContainer;
            _levelsCreator = levelsCreator;
            _levelCompletionManagement = levelCompletionManagement;

            _levelsCreator.LevelCreated
                .Subscribe(levelObject => OnLevelCreated(levelObject))
                .AddTo(Disposables);

            _levelCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(_ => CancelSpawnProcess())
                .AddTo(Disposables);

            _levelCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(_ => CancelSpawnProcess())
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables.Dispose();
            CancelSpawnProcess();
        }

        private void OnLevelCreated(LevelObject levelObject)
        {
            CancelSpawnProcess();
            UpdatePoolsByLevel(levelObject);

            _currentMovingPoints = levelObject.MovingPoints.ToList();
            _currentEnemiesInLevelSpawnSequence = levelObject.EnemysInLevelSpawnSeqence;
            _currentSpawnEnemiesPoint = levelObject.NpcSpawnPoint;

            StartSpawnProcess();
        }

        private void UpdatePoolsByLevel(LevelObject levelObject)
        {
            foreach (var enemyGroupSettings in levelObject.EnemysInLevelSpawnSeqence.EnemyGroupsSpawnSettings)
            {
                foreach (var enemySpawnSettings in enemyGroupSettings.EnemySpawnSettings)
                {
                    Enemy enemyPrefab = enemySpawnSettings.EnemyPrefab;

                    if (ObjectPoolsByEnemy.ContainsKey(enemyPrefab.EnemyName) == false)
                        ObjectPoolsByEnemy.Add(enemyPrefab.EnemyName, new ObjectPoolWithQueue<Enemy>(enemyPrefab, _enemiesContainer));
                }
            }
        }

        private async void StartSpawnProcess()
        {
            if (Application.isPlaying == false)
            {
                Debug.LogWarning($"Cannot start process when application is not playing!");
                return;
            }

            if (_spawningProcessActive)
            {
                Debug.LogWarning("Spawn process already activated!");
                return;
            }

            CancelSpawnProcess();
            _spawnCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _spawnCancellationTokenSource.Token;

            List<Enemy> createdEnemys = new();
            Vector3[] movingPositions = new Vector3[_currentMovingPoints.Count];

            for (int i = 0; i < movingPositions.Length; i++)
                movingPositions[i] = _currentMovingPoints[i].position;

            _spawningProcessActive = true;

            try
            {
                foreach (var enemysGroupSettings in _currentEnemiesInLevelSpawnSequence.EnemyGroupsSpawnSettings)
                {
                    // Await delay between spawn enemy groups
                    await UniTask.Delay(
                        Mathf.RoundToInt(enemysGroupSettings.SecondsDelayForSpawnAfterPreviousSpawn * 1000),
                        cancellationToken: cancellationToken);

                    foreach (var enemySpawnSettings in enemysGroupSettings.EnemySpawnSettings)
                    {
                        for (int i = 0; i < enemySpawnSettings.SpawnCount; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            // Spawn enemy
                            Enemy spawnedEnemy = ObjectPoolsByEnemy[enemySpawnSettings.EnemyPrefab.EnemyName].GetObject();
                            spawnedEnemy.transform.position = _currentSpawnEnemiesPoint.position;
                            spawnedEnemy.Initialize(movingPositions);

                            EnemiesInLevel.Add(spawnedEnemy, new CompositeDisposable());

                            // Subscribe to spawned enemy events
                            EnemiesInLevel[spawnedEnemy].Add(spawnedEnemy.OnDied.Subscribe(OnEnemyDied));
                            EnemiesInLevel[spawnedEnemy].Add(spawnedEnemy.OnMovedToLastPoint.Subscribe(OnEnemyMovedToLastPoint));

                            try
                            {
                                // Await delay between spawn enemies
                                await UniTask.Delay(
                                    Mathf.RoundToInt(enemySpawnSettings.SecondsDelayBetweenSpawn * 1000),
                                    cancellationToken: cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                // Its normal
                                _spawningProcessActive = false;
                                return;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Its normal
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            _spawningProcessActive = false;
        }

        private void OnEnemyMovedToLastPoint(Enemy enemy)
        {
            OnEnemyDied(enemy);

            EnemyMovedToLastPoint?.OnNext(enemy);
        }

        private void OnEnemyDied(Enemy enemy)
        {
            enemy.Dispose();

            EnemiesInLevel[enemy].Dispose();
            EnemiesInLevel.Remove(enemy);

            EnemyDied?.OnNext(enemy);

            if (ObjectPoolsByEnemy[enemy.EnemyName].ContainsObject(enemy) == false)
                ObjectPoolsByEnemy[enemy.EnemyName].AddObject(enemy);

            if (_spawningProcessActive == false && EnemiesInLevel.Count == 0)
                AllEnemiesDefeated?.OnNext(Unit.Default);
        }

        private void CancelSpawnProcess()
        {
            if (_spawnCancellationTokenSource == null)
                return;

            _spawnCancellationTokenSource.Cancel();
            _spawnCancellationTokenSource.Dispose();
            _spawnCancellationTokenSource = null;

            List<Enemy> enemiesInLevel = new();

            foreach (var keyValuePair in EnemiesInLevel)
                enemiesInLevel.Add(keyValuePair.Key);

            for (int i = 0; i < enemiesInLevel.Count; i++)
            {
                Enemy enemy = enemiesInLevel[i];

                if (enemy != null)
                {
                    enemy.Dispose();

                    EnemiesInLevel[enemy].Dispose();
                    EnemiesInLevel.Remove(enemy);

                    if (ObjectPoolsByEnemy[enemy.EnemyName].ContainsObject(enemy) == false)
                        ObjectPoolsByEnemy[enemy.EnemyName].AddObject(enemy);
                }
            }

            _spawningProcessActive = false;
            EnemiesInLevel.Clear();
        }
    }
}
