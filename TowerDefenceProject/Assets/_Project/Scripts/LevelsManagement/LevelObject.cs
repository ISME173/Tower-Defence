using _Project.Scripts.EnemiesManagement.Spawn;
using System;
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

        [Header("Stars Settings (remaining castle HP thresholds)")]
        [SerializeField, Min(0)] private int _healthFor1Star = 1;
        [SerializeField, Min(0)] private int _healthFor2Stars = 2;
        [SerializeField, Min(0)] private int _healthFor3Stars = 3;

        public int InitialAmountOfMoney => _initialAmountOfMoney;
        public GameObject CastleView => _castleView;
        public int MaxLevelHealth => _maxLevelHealth;
        public int LevelNumber => _levelNumber;
        public IReadOnlyList<Transform> MovingPoints => _movingPoints;
        public EnemysInLevelSpawnSeqence EnemysInLevelSpawnSeqence => _enemySpawnSequence;
        public Transform CameraPivot => _cameraPivot;
        public Transform NpcSpawnPoint => _npcSpawnPoint;

        public int HealthFor1Star => _healthFor1Star;
        public int HealthFor2Stars => _healthFor2Stars;
        public int HealthFor3Stars => _healthFor3Stars;

        public int CalculateStarsByRemainingHealth(int remainingHealth)
        {
            remainingHealth = Math.Max(0, remainingHealth);

            // нормализуем пороги (на случай, если в инспекторе введут не по порядку)
            int t1 = Math.Max(0, _healthFor1Star);
            int t2 = Math.Max(t1, _healthFor2Stars);
            int t3 = Math.Max(t2, _healthFor3Stars);

            if (remainingHealth >= t3) return 3;
            if (remainingHealth >= t2) return 2;
            if (remainingHealth >= t1) return 1;

            // уровень “пройден” не может быть при 0 HP, но на всякий случай:
            return 1;
        }
    }
}
