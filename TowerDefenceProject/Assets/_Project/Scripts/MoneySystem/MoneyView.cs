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

        [SerializeField] private TextMeshProUGUI _amountOfMoneyText;
        [SerializeField] private Button _onAddMoneyByAdButton;

        private MoneyManagement _moneyManagement;

        private readonly Subject<Unit> OnAddMoneyByAdButtonClicked = new();

        public ISubject<Unit> ReadOnlyOnAddMoneyByAdButtonClicked => OnAddMoneyByAdButtonClicked;

        private void Awake()
        {
            OnCurrentAmounfOfMoneyChanged(_moneyManagement.ReadOnlyCurrentAmountOfMoney.CurrentValue);
        }

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

            _onAddMoneyByAdButton.onClick.AddListener(() => OnAddMoneyByAdButtonClicked?.OnNext(Unit.Default));
        }
    }
}
