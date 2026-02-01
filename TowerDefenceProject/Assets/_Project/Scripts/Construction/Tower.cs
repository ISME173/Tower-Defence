using _Project.Scripts.EnemiesManagement;
using _Project.Scripts.Utilities;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Construction
{
    public abstract class Tower : MonoBehaviour
    {
        private readonly Dictionary<Enemy, IDisposable> EnemiesInAttackZone = new();

        [Header("References")]
        [SerializeField] private Trigger _attackZone;

        [Header("Settings")]
        [SerializeField] private List<UpgradeLevelDatas> _upgradeLevelDatas;

        private int _upgradeLevelIndex;
        private TowerData _currentTowerData;
        private Enemy _targetEnemy;
        private float _attackDelayTimer;

        protected Enemy TargetEnemy => _targetEnemy;
        protected TowerData TowerData => _currentTowerData;

        public int BuildPrice => _upgradeLevelDatas[_upgradeLevelIndex].TowerData.BuildPrice;

        protected virtual void Update()
        {
            if (_targetEnemy == null)
                return;

            _attackDelayTimer += Time.deltaTime;

            if (_attackDelayTimer >= _currentTowerData.DelayBetweenAttacks)
            {
                AttackEnemy(_targetEnemy);
                _attackDelayTimer = 0;
            }
        }

        private void OnValidate()
        {
            for (int i = 0; i < _upgradeLevelDatas.Count; i++)
            {
                TowerData towerData = _upgradeLevelDatas[i].TowerData;

                if (towerData != null && towerData.GetType() != GetTowerDataType())
                {
                    Debug.LogError($"Incorrect tower settings! Needed type: {GetTowerDataType()}. Current type: {TowerData.GetType()}");
                    _upgradeLevelDatas.Clear();
                    return;
                }
            }
        }

        public bool CanUpgrade()
        {
            return _upgradeLevelIndex + 1 < _upgradeLevelDatas.Count;
        }

        public int GetBuildPriceForNextLevel()
        {
            if (CanUpgrade() == false)
            {
                throw new Exception($"Tower '{gameObject.name}' already in last upgrade level!");
            }

            return _upgradeLevelDatas[_upgradeLevelIndex + 1].TowerData.BuildPrice;
        }

        public virtual void Upgrade()
        {
            if (CanUpgrade() == false)
            {
                Debug.LogWarning($"Tower '{gameObject.name}' already in last upgrade level!");
                return;
            }

            _upgradeLevelDatas[_upgradeLevelIndex].UpgradeTowerView.SetActive(false);
            _upgradeLevelDatas[_upgradeLevelIndex + 1].UpgradeTowerView.SetActive(true);

            _currentTowerData = _upgradeLevelDatas[_upgradeLevelIndex + 1].TowerData;

            _upgradeLevelIndex++;
        }

        public virtual void Initialize()
        {
            _upgradeLevelIndex = 0;
            _currentTowerData = _upgradeLevelDatas[_upgradeLevelIndex].TowerData;

            _attackZone.OnTriggerEnterEvent += OnTriggerEnterInAttackZone;
            _attackZone.OnTrggerExitEvent += OnTriggerExitFromAttackZone;
        }

        public virtual void Deinitialize()
        {
            _attackZone.OnTriggerEnterEvent -= OnTriggerEnterInAttackZone;
            _attackZone.OnTrggerExitEvent -= OnTriggerExitFromAttackZone;
        }

        protected abstract Type GetTowerDataType();
        protected abstract void AttackEnemy(Enemy enemy);

        private void OnTriggerEnterInAttackZone(Collider collider)
        {
            if (collider.TryGetComponent(out Enemy enemy))
            {
                if (EnemiesInAttackZone.ContainsKey(enemy) == false && enemy.Alive)
                {
                    EnemiesInAttackZone.Add(enemy, enemy.OnDied.Subscribe(OnDiedEnemy));

                    if (_targetEnemy == null)
                    {
                        _attackDelayTimer = 0;
                        _targetEnemy = enemy;
                    }
                }
            }
        }

        private void OnTriggerExitFromAttackZone(Collider collider)
        {
            if (collider.TryGetComponent(out Enemy enemy))
            {
                if (EnemiesInAttackZone.ContainsKey(enemy))
                {
                    EnemiesInAttackZone[enemy].Dispose();
                    EnemiesInAttackZone.Remove(enemy);

                    if (_targetEnemy == enemy)
                        _targetEnemy = EnemiesInAttackZone.FirstOrDefault().Key;
                }
            }
        }

        private void OnDiedEnemy(Enemy enemy)
        {
            if (EnemiesInAttackZone.ContainsKey(enemy))
            {
                EnemiesInAttackZone.Remove(enemy);

                if (enemy == _targetEnemy)
                    _targetEnemy = EnemiesInAttackZone.FirstOrDefault().Key;
            }
        }

        [Serializable]
        protected struct UpgradeLevelDatas
        {
            [SerializeField] private GameObject _upgradeTowerView;
            [Space]
            [SerializeField, Min(1)] private int _upgradeLevel;
            [SerializeField] private TowerData _towerData;

            public GameObject UpgradeTowerView => _upgradeTowerView;
            public int UpgradeLevel => _upgradeLevel;
            public TowerData TowerData => _towerData;
        }
    }
}
