using _Project.Scripts.LevelsManagement;
using _Project.Scripts.Utilities;
using Cysharp.Threading.Tasks;
using R3;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace _Project.Scripts.Enemy.EnemySpawnManagement
{
    public class EnemysSpawner : MonoBehaviour
    {
        private readonly Dictionary<Enemy, IDisposable> EnemiesInLevel = new();
        private readonly Dictionary<string, ObjectPoolWithQueue<Enemy>> ObjectPoolsByEnemy = new();

        private Transform _currentSpawnEnemiesPoint;
        private LevelsCreator _levelsCreator;
        private CompositeDisposable _compositeDisposable = new();
        private List<Transform> _currentMovingPoints;
        private EnemysInLevelSpawnSeqence _currentEnemiesInLevelSpawnSequence;
        private CancellationTokenSource _spawnCancellationTokenSource;

        private void OnEnable()
        {
            _levelsCreator.LevelCreated
                .Subscribe(levelObject => OnLevelCreated(levelObject))
                .AddTo(_compositeDisposable);
        }

        private void OnDisable()
        {
            _compositeDisposable.Dispose();
            CancelSpawn();
        }

        private void OnLevelCreated(LevelObject levelObject)
        {
            CancelSpawn();
            UpdatePoolsByLevel(levelObject);

            _currentMovingPoints = levelObject.MovingPoints.ToList();
            _currentEnemiesInLevelSpawnSequence = levelObject.EnemysInLevelSpawnSeqence;
            _currentSpawnEnemiesPoint = levelObject.NpcSpawnPoint;
        }

        private void UpdatePoolsByLevel(LevelObject levelObject)
        {
            foreach (var enemyGroupSettings in levelObject.EnemysInLevelSpawnSeqence.EnemyGroupsSpawnSettings)
            {
                foreach (var enemySpawnSettings in enemyGroupSettings.EnemySpawnSettings)
                {
                    Enemy enemyPrefab = enemySpawnSettings.EnemyPrefab;

                    if (ObjectPoolsByEnemy.ContainsKey(enemyPrefab.EnemyName) == false)
                        ObjectPoolsByEnemy.Add(enemyPrefab.EnemyName, new ObjectPoolWithQueue<Enemy>(enemyPrefab, transform));

                    //for (int i = 0; i < enemySpawnSettings.SpawnCount; i++)
                    //    ObjectPoolsByEnemy[enemyPrefab.EnemyName].AddObject(Instantiate(enemyPrefab));
                }
            }
        }

        [ContextMenu("StartSpawnProcess")]
        private async void StartSpawnProcess()
        {
            CancelSpawn();
            _spawnCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _spawnCancellationTokenSource.Token;

            List<Enemy> createdEnemys = new();
            Vector3[] movingPositions = new Vector3[_currentMovingPoints.Count];

            for (int i = 0; i < movingPositions.Length; i++)
                movingPositions[i] = _currentMovingPoints[i].position;

            try
            {
                foreach (var enemysGroupSettings in _currentEnemiesInLevelSpawnSequence.EnemyGroupsSpawnSettings)
                {
                    await UniTask.Delay(
                        Mathf.RoundToInt(enemysGroupSettings.SecondsDelayForSpawnAfterPreviousSpawn * 1000),
                        cancellationToken: cancellationToken);

                    foreach (var enemySpawnSettings in enemysGroupSettings.EnemySpawnSettings)
                    {
                        for (int i = 0; i < enemySpawnSettings.SpawnCount; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            Enemy spawnedEnemy = ObjectPoolsByEnemy[enemySpawnSettings.EnemyPrefab.EnemyName].GetObject();
                            spawnedEnemy.transform.position = _currentSpawnEnemiesPoint.position;
                            spawnedEnemy.Initialize(movingPositions);

                            EnemiesInLevel.Add(spawnedEnemy, spawnedEnemy.OnDied.Subscribe(OnEnemyDied));

                            await UniTask.Delay(
                                Mathf.RoundToInt(enemySpawnSettings.SecondsDelayBetweenSpawn * 1000),
                                cancellationToken: cancellationToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnEnemyDied(Enemy enemy)
        {
            enemy.Dispose();

            EnemiesInLevel[enemy].Dispose();
            EnemiesInLevel.Remove(enemy);

            if (ObjectPoolsByEnemy[enemy.EnemyName].ContainsObject(enemy) == false)
                ObjectPoolsByEnemy[enemy.EnemyName].AddObject(enemy);
        }

        private void CancelSpawn()
        {
            if (_spawnCancellationTokenSource == null)
                return;

            _spawnCancellationTokenSource.Cancel();
            _spawnCancellationTokenSource.Dispose();
            _spawnCancellationTokenSource = null;

            //for (int i = 0; i < EnemiesInLevel.Count; i++)
            //{
            //    if (EnemiesInLevel[i] != null)
            //    {
            //        OnEnemyDied(EnemiesInLevel[i]);
            //        Destroy(EnemiesInLevel[i].gameObject);
            //        i--;
            //    }
            //}

            List<Enemy> enemiesInLevel = new();

            foreach (var keyValuePair in EnemiesInLevel)
                enemiesInLevel.Add(keyValuePair.Key);

            for (int i = 0; i < enemiesInLevel.Count; i++)
            {
                Enemy enemy = enemiesInLevel[i];

                if (enemy != null)
                {
                    OnEnemyDied(enemiesInLevel[i]);
                }
            }

            EnemiesInLevel.Clear();
        }

        [Inject]
        private void Initialize(LevelsCreator levelsCreator)
        {
            _levelsCreator = levelsCreator;
        }
    }
}
