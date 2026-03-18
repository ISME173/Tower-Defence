using _Project.Scripts.CameraControll;
using _Project.Scripts.Castle;
using _Project.Scripts.DI;
using _Project.Scripts.EnemiesManagement.Spawn;
using _Project.Scripts.GameoverMagamenet;
using _Project.Scripts.PauseManagement;
using _Project.Scripts.Saves;
using _Project.Scripts.VictoryManagement;
using Reflex.Attributes;
using Reflex.Core;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelsManagementBindingsModule : BindingModule
    {
        [Header("References")]
        [SerializeField] private LevelsListView _levelsListView;
        [SerializeField] private Transform _createLevelPoint;
        [Space]
        [SerializeField] private LevelObject _firstLevelOnScene;
        [SerializeField] private LevelObject _firstLevelPrefab;

        [Header("Addressables (levels starting from level 2)")]
        [SerializeField] private List<string> _levelAddressKeysStartingFromLevel2 = new();

        private LevelsListController _levelsListController;
        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelsCompletionManagement;
        private AddressablesLevelsLoader _addressablesLevelsLoader;

        private LevelsProgressionService _levelsProgressionService;
        private LevelsProgressionRuntimeTracker _levelsProgressionRuntimeTracker;

        private void OnDestroy()
        {
            _levelsProgressionRuntimeTracker?.Dispose();
            _levelsProgressionService?.Dispose();

            _levelsCreator?.Dispose();
            _levelsCompletionManagement?.Dispose();
            _addressablesLevelsLoader?.Dispose();
            _levelsListController?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _addressablesLevelsLoader = new AddressablesLevelsLoader(_levelAddressKeysStartingFromLevel2);
            _addressablesLevelsLoader.WarmupAll(); // фон: начинаем грузить уровни 2+

            _levelsCreator = new LevelsCreator(_firstLevelOnScene, _firstLevelPrefab, _createLevelPoint, _addressablesLevelsLoader);
            _levelsCompletionManagement = new LevelCompletionManagement();

            _levelsProgressionService = new LevelsProgressionService();
            _levelsProgressionRuntimeTracker = new LevelsProgressionRuntimeTracker();

            _levelsListController = new LevelsListController(_levelsListView, _addressablesLevelsLoader, _levelsCreator, _levelsProgressionService);

            containerBuilder.RegisterValue(_levelsCompletionManagement);
            containerBuilder.RegisterValue(_addressablesLevelsLoader);
            containerBuilder.RegisterValue(_levelsCreator);
            containerBuilder.RegisterValue(_levelsProgressionService);
            containerBuilder.RegisterValue(_levelsListController);
        }

        [Inject]
        private void Initialize(
            ISaves saves,
            CastleHealthManagement castleHealthManagement,
            EnemiesSpawner enemysSpawner,
            GameoverController gameoverController,
            VictoryController victoryController,
            CameraMoving cameraMoving, 
            PauseController pauseController)
        {
            _levelsCompletionManagement.Initialize(castleHealthManagement, enemysSpawner, _levelsCreator, cameraMoving);
            _levelsCreator.Initialize(cameraMoving);
            _levelsProgressionService.Initialize(saves, _addressablesLevelsLoader.TotalLevelsCount);

            _levelsProgressionRuntimeTracker.Initialize(
                _levelsProgressionService,
                _levelsCreator,
                castleHealthManagement,
                _levelsCompletionManagement);

            _levelsListController.Initialize(victoryController, gameoverController, pauseController);
        }
    }
}
