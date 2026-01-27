using _Project.Scripts.Enemy.EnemySpawnManagement;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelObject : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _levelNumber;
        [Space]
        [SerializeField] private List<Transform> _movingPoints;
        [SerializeField] private EnemysInLevelSpawnSeqence _enemySpawnSequence;
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Transform _npcSpawnPoint;

        public int LevelNumber => _levelNumber;
        public IReadOnlyList<Transform> MovingPoints => _movingPoints;
        public EnemysInLevelSpawnSeqence EnemysInLevelSpawnSeqence => _enemySpawnSequence;
        public Transform CameraPivot => _cameraPivot;
        public Transform NpcSpawnPoint => _npcSpawnPoint;
    }
}
