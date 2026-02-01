using _Project.Scripts.EnemiesManagement.Spawn;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private List<Transform> _movingPoints;
        [SerializeField] private EnemysInLevelSpawnSeqence _enemySpawnSequence;
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Transform _npcSpawnPoint;
        [SerializeField] private GameObject _castleView;

        [Header("Settings")]
        [SerializeField, Min(1)] private int _levelNumber;
        [SerializeField, Min(0)] private int _maxLevelHealth;
        [SerializeField, Min(0)] private int _initialAmountOfMoney;

        public int InitialAmountOfMoney => _initialAmountOfMoney;
        public GameObject CastleView => _castleView;
        public int MaxLevelHealth => _maxLevelHealth;
        public int LevelNumber => _levelNumber;
        public IReadOnlyList<Transform> MovingPoints => _movingPoints;
        public EnemysInLevelSpawnSeqence EnemysInLevelSpawnSeqence => _enemySpawnSequence;
        public Transform CameraPivot => _cameraPivot;
        public Transform NpcSpawnPoint => _npcSpawnPoint;
    }
}
