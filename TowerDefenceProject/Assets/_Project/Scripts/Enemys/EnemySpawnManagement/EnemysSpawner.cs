using _Project.Scripts.LevelsManagement;
using Cysharp.Threading.Tasks;
using R3;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace _Project.Scripts.Enemy.EnemySpawnManagement
{
    public class EnemysSpawner : MonoBehaviour
    {
        private readonly List<Enemy> EnemiesInLevel = new();

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

            _currentMovingPoints = levelObject.MovingPoints.ToList();
            _currentEnemiesInLevelSpawnSequence = levelObject.EnemysInLevelSpawnSeqence;
        }

        [ContextMenu("StartSpawnProcess")]
        private async void StartSpawnProcess()
        {
            CancelSpawn();
            _spawnCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _spawnCancellationTokenSource.Token;

            List<Enemy> createdEnemys = new();
            Vector3[] movingPositions = new Vector3[_currentMovingPoints.Count - 1];

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

                            Enemy spawnedEnemy = Instantiate(enemySpawnSettings.EnemyPrefab);
                            spawnedEnemy.Initialize(movingPositions);

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

        private void CancelSpawn()
        {
            if (_spawnCancellationTokenSource == null)
                return;

            _spawnCancellationTokenSource.Cancel();
            _spawnCancellationTokenSource.Dispose();
            _spawnCancellationTokenSource = null;

            for (int i = 0; i < EnemiesInLevel.Count; i++)
            {
                if (EnemiesInLevel[i] != null)
                    Destroy(EnemiesInLevel[i].gameObject);
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
