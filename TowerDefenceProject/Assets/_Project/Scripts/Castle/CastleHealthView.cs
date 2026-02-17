using _Project.Scripts.Utilities;
using AnimationsUI.CoreScripts;
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
        [SerializeField] private RectTransform _viewForHideAndShow;
        [Space]
        [SerializeField] private Button _getHeartsButton;
        [SerializeField] private RectTransform _getHeartsAfterWatchAdvPanel;
        [SerializeField] private Button _watchAdvButton;
        [SerializeField] private Button _noWatchAdvButton;
        [Space]
        [SerializeField] private PopupAnimationPanelsSequence _getHeartsPanelAnimation;

        [Header("Settings")]
        [SerializeField] private LMotionShakeSerializableSettings _shakeHeartSettings; 

        private MotionHandle _heartIconShakeHandle;
        private CastleHealthManagement _castleHealthManagement;

        private readonly Subject<Unit> OnWatchAdvButtonClicked = new(), OnNoWatchAdvButtonClicked = new(), OnGetHeartsButtonClicked = new();

        public Observable<Unit> ReadOnlyOnWatchAdvButtonClicked => OnWatchAdvButtonClicked;
        public Observable<Unit> ReadOnlyOnNoWatchAdvButtonClicked => OnNoWatchAdvButtonClicked;
        public Observable<Unit> ReadOnlyOnGetHeartsButtonClicked => OnGetHeartsButtonClicked;

        public void Dispose()
        {
            Disposables.Dispose();
            
            OnWatchAdvButtonClicked.OnCompleted();
            OnNoWatchAdvButtonClicked.OnCompleted();
        }

        public void ShowMainView()
        {
            _viewForHideAndShow.gameObject.SetActive(true);
        }

        public void HideMainView()
        {
            _viewForHideAndShow.gameObject.SetActive(false);
        }

        public void ShowWatchAdvForGetHeartsPanel()
        {
            _getHeartsAfterWatchAdvPanel.gameObject.SetActive(true);
            _getHeartsPanelAnimation.Show(null);
        }

        public void HideWatchAdvForGetHeartsPanel()
        {
            _getHeartsPanelAnimation.Hide(() => _getHeartsAfterWatchAdvPanel.gameObject.SetActive(false));
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

            _getHeartsButton.onClick.AddListener(() => OnGetHeartsButtonClicked?.OnNext(Unit.Default));
            _watchAdvButton.onClick.AddListener(() => OnWatchAdvButtonClicked?.OnNext(Unit.Default));
            _noWatchAdvButton.onClick.AddListener(() => OnNoWatchAdvButtonClicked?.OnNext(Unit.Default));
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
