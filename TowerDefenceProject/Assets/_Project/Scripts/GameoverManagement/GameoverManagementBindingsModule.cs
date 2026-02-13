using _Project.Scripts.DI;
using _Project.Scripts.LevelsManagement;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.GameoverMagamenet
{
    public class GameoverManagementBindingsModule : BindingModule
    {
        [SerializeField] private GameoverView _gameoverView;

        private GameoverController _gameoverController;

        private void OnDestroy()
        {
            _gameoverController?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _gameoverController = new GameoverController(_gameoverView);
            containerBuilder.RegisterValue(_gameoverController);
        }

        [Inject]
        private void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement completionManagement)
        {
            _gameoverController.Initialize(levelsCreator, completionManagement);
        }
    }
}
