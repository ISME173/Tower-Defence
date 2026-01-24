using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Enemy.EnemySpawnManagement
{
    public class EnemysSpawner : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform[] _movingPoints;

        [Header("Settings")]
        [SerializeField] private EnemysInLevelSpawnSeqence _enemysInLevelSpawnSeqence;

        [ContextMenu("StartSpawnProcess")]
        private async void StartSpawnProcess()
        {
            List<Enemy> createdEnemys = new();
            Vector3[] movingPositions = new Vector3[_movingPoints.Length];

            for (int i = 0; i < movingPositions.Length; i++)
                movingPositions[i] = _movingPoints[i].position;

            foreach (var enemysGroupSettings in _enemysInLevelSpawnSeqence.EnemyGroupsSpawnSettings)
            {
                await UniTask.Delay(Mathf.RoundToInt(enemysGroupSettings.SecondsDelayForSpawnAfterPreviousSpawn * 1000));

                foreach (var enemySpawnSettings in enemysGroupSettings.EnemySpawnSettings)
                {
                    for (int i = 0; i < enemySpawnSettings.SpawnCount; i++)
                    {
                        Enemy spawnedEnemy = Instantiate(enemySpawnSettings.EnemyPrefab);
                        spawnedEnemy.Initialize(movingPositions);

                        await UniTask.Delay(Mathf.RoundToInt(enemySpawnSettings.SecondsDelayBetweenSpawn * 1000));
                    }
                }
            }
        }
    }
}
