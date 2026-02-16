using _Project.Scripts.Castle;
using _Project.Scripts.DI;
using _Project.Scripts.EnemiesManagement.Spawn;
using Reflex.Attributes;
using Reflex.Core;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelsManagementBindingsModule : BindingModule
    {
        [SerializeField] private Transform _createLevelPoint;
        [SerializeField] private LevelObject[] _levelObjectPrefabs;

        [Header("Addressables (levels starting from level 2)")]
        [SerializeField] private List<string> _levelAddressKeysStartingFromLevel2 = new();

        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelsCompletionManagement;
        private AddressablesLevelsLoader _addressablesLevelsLoader;

        private void OnDestroy()
        {
            _levelsCreator?.Dispose();
            _levelsCompletionManagement?.Dispose();
            _addressablesLevelsLoader?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _addressablesLevelsLoader = new AddressablesLevelsLoader(_levelAddressKeysStartingFromLevel2);
            _addressablesLevelsLoader.WarmupAll(); // фон: начинаем грузить уровни 2+

            _levelsCreator = new LevelsCreator(_levelObjectPrefabs, _createLevelPoint, _addressablesLevelsLoader);
            _levelsCompletionManagement = new LevelCompletionManagement();

            containerBuilder.RegisterValue(_levelsCompletionManagement);
            containerBuilder.RegisterValue(_addressablesLevelsLoader);
            containerBuilder.RegisterValue(_levelsCreator);
        }

        [Inject]
        private void Initialize(CastleHealthManagement castleHealthManagement, EnemiesSpawner enemysSpawner)
        {
            _levelsCompletionManagement.Initialize(castleHealthManagement, enemysSpawner);
        }
    }
}
