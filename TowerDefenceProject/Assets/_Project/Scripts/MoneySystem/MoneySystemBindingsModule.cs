using _Project.Scripts.DI;
using _Project.Scripts.EnemiesManagement.Spawn;
using _Project.Scripts.LevelsManagement;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.MoneySystem
{
    public class MoneySystemBindingsModule : BindingModule
    {
        [SerializeField] private MoneyView _moneyView;

        private MoneyManagement _moneyManagement;

        private void OnDestroy()
        {
            _moneyManagement?.Dispose();
            _moneyView?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _moneyManagement = new();

            containerBuilder.RegisterValue(_moneyManagement);
            containerBuilder.RegisterValue(_moneyView);
        }

        [Inject]
        private void Initialize(EnemiesSpawner enemiesSpawner, LevelsCreator levelsCreator)
        {
            _moneyManagement.Initialze(enemiesSpawner, levelsCreator);
        }
    }
}
