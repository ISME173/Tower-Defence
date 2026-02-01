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
        [SerializeField, Min(0)] private int _attackDamage;
        [SerializeField, Min(0)] private float _delayBetweenAttacks;
        [SerializeField, Min(0)] private float _rotateWeaponToEnemySpeed;
        [Space]
        [SerializeField, Min(0)] private int _buildPrice;

        private Enemy _targetEnemy;
        private float _attackDelayTimer;

        protected Enemy TargetEnemy => _targetEnemy;
        protected int AttackDamage => _attackDamage;
        protected float RotateWeaponToEnemySpeed => _rotateWeaponToEnemySpeed;

        public int BuildPrice => _buildPrice;

        protected virtual void Update()
        {
            if (_targetEnemy == null)
                return;

            _attackDelayTimer += Time.deltaTime;

            if (_attackDelayTimer >= _delayBetweenAttacks)
            {
                AttackEnemy(_targetEnemy);
                _attackDelayTimer = 0;
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
