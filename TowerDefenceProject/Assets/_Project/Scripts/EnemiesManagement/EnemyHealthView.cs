using R3;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.EnemiesManagement
{
    public class EnemyHealthView : MonoBehaviour
    {
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Enemy _targetEnemy;

        private Transform _transform;
        private Camera _mainCamera;
        private IDisposable _disposable;

        private void Awake()
        {
            _transform = transform;
            _mainCamera = Camera.main;
            _healthBar.interactable = false;
            _healthBar.maxValue = _targetEnemy.MaxHealth;
        }

        private void OnEnable()
        {
            _disposable = _targetEnemy.ReadOnlyCurrentHealth.Subscribe(OnHealthChanged);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            _transform.LookAt(_transform.position + _mainCamera.transform.forward);
        }

        private void OnHealthChanged(int health)
        {
            _healthBar.value = health;
        }
    }
}
