using LitMotion;
using LitMotion.Extensions;
using R3;
using Reflex.Attributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.MoneySystem
{
    public class MoneyView : MonoBehaviour, IDisposable
    {
        private readonly CompositeDisposable Disposables = new();

        [Header("References")]
        [SerializeField] private TextMeshProUGUI _amountOfMoneyText;
        [SerializeField] private Button _onAddMoneyByAdButton;
        [SerializeField] private RectTransform _moneyIcon;
        [Space]
        [SerializeField] private Button _getMoneyButton;
        [SerializeField] private RectTransform _getMoneyAfterWatchAdvPanel;
        [SerializeField] private Button _watchAdvButton;
        [SerializeField] private Button _noWatchAdvButton;

        [Header("Settings")]
        [SerializeField] private Vector3 _moneyIconIncreasedLocalScale;
        [SerializeField, Min(0)] private float _moneyIncreaseDuration;
        [SerializeField, Min(0)] private Ease _moneyIncreaseEase;
        [Space]
        [SerializeField] private Vector3 _moneyIconReductionLocalScale;
        [SerializeField, Min(0)] private float _moneyReductionDuration;
        [SerializeField, Min(0)] private Ease _moneyReductionEase;

        private MotionHandle _moneyIconIncreaseHandle;
        private MoneyManagement _moneyManagement;

        private readonly Subject<Unit> OnWatchAdvButtonClicked = new(), OnNoWatchAdvButtonClicked = new(), OnGetMoneyButtonClicked = new();

        public Observable<Unit> ReadOnlyOnWatchAdvButtonClicked => OnWatchAdvButtonClicked;
        public Observable<Unit> ReadOnlyOnNoWatchAdvButtonClicked => OnNoWatchAdvButtonClicked;
        public Observable<Unit> ReadOnlyOnGetMoneyButtonClicked => OnGetMoneyButtonClicked;

        public void Dispose()
        {
            Disposables.Dispose();

            OnWatchAdvButtonClicked.OnCompleted();
            OnNoWatchAdvButtonClicked.OnCompleted();
        }

        public void ShowWatchAdvForGetMoneyPanel()
        {
            _getMoneyAfterWatchAdvPanel.gameObject.SetActive(true);
        }

        public void HideWatchAdvForGetMoneyPanel()
        {
            _getMoneyAfterWatchAdvPanel.gameObject.SetActive(false);
        }


        private void OnCurrentAmounfOfMoneyChanged(int amountOfMoney)
        {
            _amountOfMoneyText.text = amountOfMoney.ToString();
        }

        [Inject]
        private void Initialize(MoneyManagement moneyManagement)
        {
            _moneyManagement = moneyManagement;

            _moneyManagement.ReadOnlyCurrentAmountOfMoney
                .Subscribe(OnCurrentAmounfOfMoneyChanged)
                .AddTo(Disposables);

            _moneyManagement.ReadOnlyOnMoneysAdded
                .Subscribe(OnMoneysAdded)
                .AddTo(Disposables);

            _getMoneyButton.onClick.AddListener(() => OnGetMoneyButtonClicked?.OnNext(Unit.Default));
            _watchAdvButton.onClick.AddListener(() => OnWatchAdvButtonClicked?.OnNext(Unit.Default));
            _noWatchAdvButton.onClick.AddListener(() => OnNoWatchAdvButtonClicked?.OnNext(Unit.Default));
        }

        private void OnMoneysAdded(Unit unit)
        {
            _moneyIconIncreaseHandle.TryCancel();

            MotionSequenceBuilder motionSequenceBuilder = LSequence.Create();

            motionSequenceBuilder.Append(LMotion.Create(_moneyIcon.transform.localScale, _moneyIconIncreasedLocalScale, _moneyIncreaseDuration)
                .BindToLocalScale(_moneyIcon))
                .Append(LMotion.Create(_moneyIcon.transform.localScale, _moneyIconReductionLocalScale, _moneyReductionDuration)
                .BindToLocalScale(_moneyIcon));

            _moneyIconIncreaseHandle = motionSequenceBuilder.Run();
        }
    }
}
