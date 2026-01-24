using LitMotion;
using LitMotion.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Enemy
{
    [RequireComponent(typeof(Collider), typeof(Animator))]
    public class Enemy : MonoBehaviour
    {
        private readonly List<Vector3> MovingPoints = new();

        [Header("Base enemy settings")]
        [SerializeField, Min(0)] private int _maxHealth;
        [SerializeField, Min(0)] private float _movingSpeed;
        [SerializeField, Min(0)] private int _attackDamage;

        [Header("Base enemy animation states")]
        [SerializeField] private string _isWalkingStateName;

        private int _currentPointIndex;
        private int _currentHealth;
        private MotionHandle _movingHandle;
        private Transform _transform;
        private Collider _collider;

        public event Action<Enemy> OnDied;

        protected event Action OnMovedEvent;

        public Transform Transform => _transform ?? transform;
        public int AttackDamage => _attackDamage;

        public virtual void Initialize(Vector3[] movingPoints)
        {
            _collider = GetComponent<Collider>();
            _transform = transform;

            _collider.isTrigger = true;
            _currentHealth = _maxHealth;

            MovingPoints.AddRange(movingPoints);
            _currentPointIndex = 0;

            OnMovedEvent += OnMoved;
            MoveToPoint(MovingPoints[_currentPointIndex]);
        }

        public virtual void Deinitialize()
        {
            _movingHandle.TryCancel();

            OnMovedEvent -= OnMoved;
        }

        public virtual void TakeDamage(int damage)
        {
            float finalDamage = Math.Clamp(damage, 0, _currentHealth);

            _currentHealth -= damage;

            if (_currentHealth == 0)
                Died();
        }

        protected virtual void MoveToPoint(Vector3 movePosition)
        {
            Vector3 direction = movePosition - Transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.0001f)
                Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            float movingTime = Math.Max(Vector3.Distance(movePosition, Transform.position) / _movingSpeed, 0);
            _movingHandle = LMotion.Create(Transform.position, movePosition, movingTime)
                .WithCancelOnError()
                .WithOnComplete(() => OnMovedEvent?.Invoke())
                .BindToPosition(Transform);
        }

        protected virtual void Died()
        {
            _movingHandle.TryCancel();
            OnDied?.Invoke(this);
        }

        private void OnMoved()
        {
            _currentPointIndex++;

            if (_currentPointIndex < MovingPoints.Count)
                MoveToPoint(MovingPoints[_currentPointIndex]);
        }
    }
}
