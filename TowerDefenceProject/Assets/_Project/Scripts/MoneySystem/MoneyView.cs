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

        private readonly Subject<Unit> OnAddMoneyByAdButtonClicked = new();

        public ISubject<Unit> ReadOnlyOnAddMoneyByAdButtonClicked => OnAddMoneyByAdButtonClicked;

        public void Dispose()
        {
            Disposables.Dispose();
            OnAddMoneyByAdButtonClicked?.Dispose();
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

            _onAddMoneyByAdButton.onClick.AddListener(() => OnAddMoneyByAdButtonClicked?.OnNext(Unit.Default));
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
