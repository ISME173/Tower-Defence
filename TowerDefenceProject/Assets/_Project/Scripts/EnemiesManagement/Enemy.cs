using _Project.Scripts.Construction;
using LitMotion;
using LitMotion.Extensions;
using NaughtyAttributes;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.EnemiesManagement
{
    [RequireComponent(typeof(Collider), typeof(Animator))]
    public abstract class Enemy : MonoBehaviour, IDisposable
    {
        private readonly List<Vector3> MovingPoints = new();

        [Header("Base enemy settings")]
        [SerializeField] private string _enemyName;
        [Space]
        [SerializeField, Min(0)] private int _maxHealth;
        [SerializeField, Min(0)] private float _movingSpeed;
        [SerializeField, Min(0)] private int _attackDamage;
        [Space]
        [SerializeField, Min(0)] private int _moneyForMurder;
        [Space]
        [SerializeField, Min(0)] private float _rotationTime = 0.1f;

        [Header("References")]
        [SerializeField] private ParticleSystem _diedEffect;
        [SerializeField] private Transform _center;
        [Space]
        [InfoBox("Can be left blank")]
        [SerializeField] private List<TowerAndDamageOffset> _vulnerabilityToTowerPrefabsList;
        [SerializeField] private List<TowerAndDamageOffset> _resistanceToTowerPrefabsList;

        private int _currentPointIndex;
        private MotionHandle _diedEffectLifeHandle;
        private MotionHandle _movingHandle;
        private MotionHandle _rotationHandle;
        private Transform _transform;
        private Collider _collider;

        private readonly ReactiveProperty<int> CurrentHealth = new();

        public ReadOnlyReactiveProperty<int> ReadOnlyCurrentHealth => CurrentHealth;

        public readonly Subject<Enemy> OnDied = new(), OnMovedToLastPoint = new();

        protected event Action OnMovedEvent;

        public int MoneyForMurder => _moneyForMurder;
        public string EnemyName => _enemyName;
        public Transform Transform => _transform ?? transform;
        public Transform Center => _center ?? Transform;
        public int AttackDamage => _attackDamage;
        public bool Alive => CurrentHealth.Value > 0;
        public int MaxHealth => _maxHealth;

        public virtual void Initialize(Vector3[] movingPoints)
        {
            _collider = GetComponent<Collider>();
            _transform = transform;

            _collider.isTrigger = true;
            CurrentHealth.Value = _maxHealth;

            MovingPoints.Clear();
            MovingPoints.AddRange(movingPoints);
            _currentPointIndex = 0;

            OnMovedEvent += OnMoved;
            MoveToPoint(MovingPoints[_currentPointIndex]);
        }

        public virtual void Dispose()
        {
            _movingHandle.TryCancel();
            _rotationHandle.TryCancel();

            OnMovedEvent -= OnMoved;
        }

        public virtual void TakeDamage(int damage, Tower attackTower)
        {
            if (CurrentHealth.Value == 0)
                return;

            int modifiedDamage = damage;

            // Проверка на уязвимость к типу башни
            TowerAndDamageOffset vulnerability = _vulnerabilityToTowerPrefabsList
                .FirstOrDefault(x => x.TowerPrefab.Name == attackTower.Name);
            
            if (vulnerability.TowerPrefab != null)
            {
                int additionalDamage = (damage * vulnerability.PercentageDamageOffset) / 100;
                modifiedDamage += additionalDamage;
                //Debug.Log($"Enemy {gameObject.name} is vulnerable to {attackTower.name}. Damage increased by {vulnerability.PercentageDamageOffset}%");
            }

            // Проверка на сопротивление к типу башни
            TowerAndDamageOffset resistance = _resistanceToTowerPrefabsList
                .FirstOrDefault(x => x.TowerPrefab.Name == attackTower.Name);
            
            if (resistance.TowerPrefab != null)
            {
                int reducedDamage = (damage * resistance.PercentageDamageOffset) / 100;
                modifiedDamage += reducedDamage;
                //Debug.Log($"Enemy {gameObject.name} has resistance to {attackTower.name}. Damage reduced by {resistance.PercentageDamageOffset}%");
            }

            int finalDamage = Math.Clamp(modifiedDamage, 0, CurrentHealth.Value);

            //Debug.Log($"Enemy {gameObject.name} damage taken: {finalDamage}");

            CurrentHealth.Value -= finalDamage;

            if (CurrentHealth.Value == 0)
                Died();
        }

        protected virtual void MoveToPoint(Vector3 movePosition)
        {
            Vector3 direction = movePosition - Transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                _rotationHandle.TryCancel();
                _rotationHandle = LMotion.Create(Transform.rotation, targetRotation, _rotationTime)
                    .WithCancelOnError()
                    .BindToRotation(Transform);
            }

            float movingTime = Math.Max(Vector3.Distance(movePosition, Transform.position) / _movingSpeed, 0);
            _movingHandle = LMotion.Create(Transform.position, movePosition, movingTime)
                .WithCancelOnError()
                .WithOnComplete(() => OnMovedEvent?.Invoke())
                .BindToPosition(Transform);
        }

        protected virtual void Died()
        {
            _movingHandle.TryCancel();
            _rotationHandle.TryCancel();

            _diedEffect.transform.SetParent(null);
            _diedEffect.Play();

            _diedEffectLifeHandle.TryCancel();

            _diedEffectLifeHandle = LMotion.Create(0f, 1f, _diedEffect.main.duration)
                .WithOnComplete(() => _diedEffect.transform.SetParent(transform))
                .RunWithoutBinding();

            OnDied?.OnNext(this);
        }

        private void OnMoved()
        {
            _currentPointIndex++;

            if (_currentPointIndex < MovingPoints.Count)
                MoveToPoint(MovingPoints[_currentPointIndex]);
            else
                OnMovedToLastPoint?.OnNext(this);
        }

        [Serializable]
        private struct TowerAndDamageOffset
        {
            [SerializeField] private Tower _towerPrefab;
            [SerializeField, Range(-100, 100)] private int _percentageDamageOffset;

            public Tower TowerPrefab => _towerPrefab;
            public int PercentageDamageOffset => _percentageDamageOffset;
        }
    }
}
