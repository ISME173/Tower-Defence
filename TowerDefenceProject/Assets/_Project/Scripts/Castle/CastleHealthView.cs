using R3;
using Reflex.Attributes;
using System;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Castle
{
    public class CastleHealthView : MonoBehaviour, IDisposable
    {
        [SerializeField] private TextMeshProUGUI _currentHealthText;

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
            _currentHealthText.text = currentHealth.ToString();
        }
    }
}
