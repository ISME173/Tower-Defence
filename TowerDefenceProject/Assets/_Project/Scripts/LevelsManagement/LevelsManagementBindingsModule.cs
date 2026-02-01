using _Project.Scripts.Castle;
using _Project.Scripts.DI;
using _Project.Scripts.EnemiesManagement.Spawn;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelsManagementBindingsModule : BindingModule
    {
        [SerializeField] private Transform _createLevelPoint;
        [SerializeField] private LevelObject[] _levelObjectPrefabs;

        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelsCompletionManagement;

        private void OnDestroy()
        {
            _levelsCreator?.Dispose();
            _levelsCompletionManagement?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _levelsCreator = new LevelsCreator(_levelObjectPrefabs, _createLevelPoint);
            _levelsCompletionManagement = new LevelCompletionManagement();

            containerBuilder.RegisterValue(_levelsCompletionManagement);
            containerBuilder.RegisterValue(_levelsCreator);
        }

        [Inject]
        private void Initialize(CastleHealthManagement castleHealthManagement, EnemiesSpawner enemysSpawner)
        {
            _levelsCompletionManagement.Initialize(castleHealthManagement , enemysSpawner);
        }
    }
}
