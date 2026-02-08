using R3;
using Reflex.Attributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Castle
{
    public class CastleHealthView : MonoBehaviour, IDisposable
    {
        [SerializeField] private TextMeshProUGUI _currentHealthText;
        [SerializeField] private Button _addHealthByAdButton;

        private IDisposable _disposable;
        private CastleHealthManagement _castleHealthManagement;

        private readonly Subject<Unit> OnAddHealthByAdButtonClicked = new();

        public ISubject<Unit> ReadOnlyOnAddHealthByAdButtonClicked => OnAddHealthByAdButtonClicked;

        public void Dispose()
        {
            _disposable?.Dispose();
            OnAddHealthByAdButtonClicked?.Dispose();
        }

        [Inject]
        private void Initialize(CastleHealthManagement castleHealthManagement)
        {
            _castleHealthManagement = castleHealthManagement;
            _disposable = _castleHealthManagement.ReadOnlyCurrentHealth.Subscribe(OnCastleHealthChanged);

            _addHealthByAdButton.onClick.AddListener(() => OnAddHealthByAdButtonClicked?.OnNext(Unit.Default));
        }

        private void OnCastleHealthChanged(int currentHealth)
        {
            _currentHealthText.text = currentHealth.ToString();
        }
    }
}
