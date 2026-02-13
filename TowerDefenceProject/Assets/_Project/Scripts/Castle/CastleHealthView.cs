using _Project.Scripts.Utilities;
using LitMotion;
using LitMotion.Extensions;
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
        private readonly CompositeDisposable Disposables = new();

        [Header("References")]
        [SerializeField] private TextMeshProUGUI _currentHealthText;
        [SerializeField] private Button _addHealthByAdButton;
        [SerializeField] private RectTransform _heartIcon;

        [Header("Settings")]
        [SerializeField] private LMotionShakeSerializableSettings _shakeHeartSettings; 

        private MotionHandle _heartIconShakeHandle;
        private CastleHealthManagement _castleHealthManagement;

        private readonly Subject<Unit> OnAddHealthByAdButtonClicked = new();

        public ISubject<Unit> ReadOnlyOnAddHealthByAdButtonClicked => OnAddHealthByAdButtonClicked;

        public void Dispose()
        {
            Disposables.Dispose();
            OnAddHealthByAdButtonClicked?.Dispose();
        }

        [Inject]
        private void Initialize(CastleHealthManagement castleHealthManagement)
        {
            _castleHealthManagement = castleHealthManagement;

            _castleHealthManagement.ReadOnlyCurrentHealth
                .Subscribe(OnCastleHealthChanged)
                .AddTo(Disposables);

            _castleHealthManagement.ReadOnlyCastleDamageTaken
                .Subscribe(OnCastleDamageTaken)
                .AddTo(Disposables);

            _addHealthByAdButton.onClick.AddListener(() => OnAddHealthByAdButtonClicked?.OnNext(Unit.Default));
        }

        private void OnCastleDamageTaken(int currentHealth)
        {
            _heartIconShakeHandle.TryCancel();

            _heartIconShakeHandle = LMotion.Shake.Create(_heartIcon.position.x, _shakeHeartSettings.Strenght, _shakeHeartSettings.Duration)
                .WithCancelOnError()
                .BindToPositionX(_heartIcon);
        }

        private void OnCastleHealthChanged(int currentHealth)
        {
            _currentHealthText.text = currentHealth.ToString();
        }
    }
}
