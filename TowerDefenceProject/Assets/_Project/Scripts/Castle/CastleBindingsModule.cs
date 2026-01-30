using _Project.Scripts.DI;
using _Project.Scripts.Enemy.EnemySpawnManagement;
using _Project.Scripts.LevelsManagement;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.Castle
{
    public class CastleBindingsModule : BindingModule
    {
        [SerializeField] private CastleHealthView _castleHealthView;

        private CastleHealthManagement _castleHealthManagement;

        private void OnDestroy()
        {
            _castleHealthManagement?.Dispose();
            _castleHealthView?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _castleHealthManagement = new CastleHealthManagement();

            containerBuilder.RegisterValue(_castleHealthManagement);
            containerBuilder.RegisterValue(_castleHealthView);
        }

        [Inject]
        private void Initialize(LevelsCreator levelsCreator, EnemysSpawner enemysSpawner)
        {
            _castleHealthManagement.Initialize(enemysSpawner, levelsCreator);
        }
    }
}
