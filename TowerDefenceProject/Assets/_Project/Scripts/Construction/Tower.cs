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
        [SerializeField] private TowerData _towerData;

        private Enemy _targetEnemy;
        private float _attackDelayTimer;

        protected Enemy TargetEnemy => _targetEnemy;
        protected TowerData TowerData => _towerData;

        public int BuildPrice => _towerData.BuildPrice;

        protected virtual void Update()
        {
            if (_targetEnemy == null)
                return;

            _attackDelayTimer += Time.deltaTime;

            if (_attackDelayTimer >= _towerData.DelayBetweenAttacks)
            {
                AttackEnemy(_targetEnemy);
                _attackDelayTimer = 0;
            }
        }

        private void OnValidate()
        {
            if (TowerData != null && TowerData.GetType() != GetTowerDataType())
            {
                Debug.LogError($"Incorrect tower settings! Needed type: {GetTowerDataType()}. Current type: {TowerData.GetType()}");
                _towerData = null;
                return;
            }
        }

        public virtual void Initialize()
        {
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
    }
}
