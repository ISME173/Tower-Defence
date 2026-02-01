using R3;
using Reflex.Attributes;
using System;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.MoneySystem
{
    public class MoneyView : MonoBehaviour, IDisposable
    {
        private readonly CompositeDisposable Disposables = new();

        [SerializeField] private TextMeshProUGUI _amountOfMoneyText;

        private MoneyManagement _moneyManagement;

        private void Awake()
        {
            OnCurrentAmounfOfMoneyChanged(_moneyManagement.ReadOnlyCurrentAmountOfMoney.CurrentValue);
        }

        public void Dispose()
        {
            Disposables.Dispose();
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
        }
    }
}
