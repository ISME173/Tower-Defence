using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.EnemiesManagement.Spawn
{
    [CreateAssetMenu(fileName = "EnemyGroupSpawnSettings", menuName = "EnemySpawnManagement/EnemyGroupSpawnSettings")]
    public class EnemysInLevelSpawnSeqence : ScriptableObject
    {
        [SerializeField] private List<EnemyGroupSpawnSettings> _enemyGroupsSpawnSettings;

        public IReadOnlyList<EnemyGroupSpawnSettings> EnemyGroupsSpawnSettings => _enemyGroupsSpawnSettings;

        [Serializable]
        public struct EnemyGroupSpawnSettings
        {
            [SerializeField] private float _secondsDelayForSpawnAfterPreviousSpawn;
            [SerializeField] private List<EnemySpawnSettings> _enemySpawnSettings;

            public float SecondsDelayForSpawnAfterPreviousSpawn => _secondsDelayForSpawnAfterPreviousSpawn;
            public IReadOnlyList<EnemySpawnSettings> EnemySpawnSettings => _enemySpawnSettings;
        }


        [Serializable]
        public struct EnemySpawnSettings
        {
            [SerializeField] private Enemy _enemyPrefab;
            [SerializeField, Min(1)] private int _spawnCount;
            [SerializeField, Min(0)] private float _secondsDelayBetweenSpawn; 

            public Enemy EnemyPrefab => _enemyPrefab;
            public int SpawnCount => _spawnCount;
            public float SecondsDelayBetweenSpawn => _secondsDelayBetweenSpawn;
        }
    }
}
