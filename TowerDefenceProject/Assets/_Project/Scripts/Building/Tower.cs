using _Project.Scripts.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Building
{
    public abstract class Tower : MonoBehaviour
    {
        private readonly List<Enemy.Enemy> EnemiesInAttackZone = new();

        [Header("References")]
        [SerializeField] private Trigger _attackZone;

        [Header("Settings")]
        [SerializeField, Min(0)] private int _attackDamage;
        [SerializeField, Min(0)] private float _delayBetweenAttacks;

        private Enemy.Enemy _targetEnemy;
        private float _attackDelayTimer;

        protected Enemy.Enemy TargetEnemy => _targetEnemy;
        protected int AttackDamage => _attackDamage;

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

        protected abstract void AttackEnemy(Enemy.Enemy enemy);

        private void OnTriggerEnterInAttackZone(Collider collider)
        {
            if (collider.TryGetComponent(out Enemy.Enemy enemy))
            {
                if (EnemiesInAttackZone.Contains(enemy) == false)
                {
                    EnemiesInAttackZone.Add(enemy);

                    enemy.OnDied += OnDiedEnemy;

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
            if (collider.TryGetComponent(out Enemy.Enemy enemy))
            {
                if (EnemiesInAttackZone.Contains(enemy))
                {
                    EnemiesInAttackZone.Remove(enemy);

                    enemy.OnDied -= OnDiedEnemy;

                    if (_targetEnemy == enemy)
                        _targetEnemy = EnemiesInAttackZone.FirstOrDefault();
                }
            }
        }

        private void OnDiedEnemy(Enemy.Enemy enemy)
        {
            if (EnemiesInAttackZone.Contains(enemy))
            {
                EnemiesInAttackZone.Remove(enemy);

                if (enemy == _targetEnemy)
                    _targetEnemy = EnemiesInAttackZone.FirstOrDefault();
            }
        }
    }
}
