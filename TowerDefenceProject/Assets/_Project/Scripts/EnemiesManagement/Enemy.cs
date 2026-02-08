using LitMotion;
using LitMotion.Extensions;
using R3;
using System;
using System.Collections.Generic;
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
        [SerializeField] private Transform _center;

        private int _currentPointIndex;
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

        public virtual void TakeDamage(int damage)
        {
            if (CurrentHealth.Value == 0)
                return;

            int finalDamage = Math.Clamp(damage, 0, CurrentHealth.Value);

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
    }
}
