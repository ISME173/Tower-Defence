using R3;
using Reflex.Attributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Castle
{
    public class CastleHealthView : MonoBehaviour, IDisposable
    {
        [SerializeField] private Image _healthImageByFilling;

        private IDisposable _disposable;
        private CastleHealthManagement _castleHealthManagement;

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        [Inject]
        private void Initialize(CastleHealthManagement castleHealthManagement)
        {
            _castleHealthManagement = castleHealthManagement;

            _disposable = _castleHealthManagement.ReadOnlyCurrentHealth.Subscribe(OnCastleHealthChanged);
        }

        private void OnCastleHealthChanged(int currentHealth)
        {
            int maxHealth =  _castleHealthManagement.MaxHealth;

            _healthImageByFilling.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}
