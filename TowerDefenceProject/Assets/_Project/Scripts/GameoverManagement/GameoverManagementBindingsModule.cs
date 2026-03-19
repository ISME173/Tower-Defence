using _Project.Scripts.Audio;
using _Project.Scripts.DI;
using _Project.Scripts.LevelsManagement;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.GameoverMagamenet
{
    public class GameoverManagementBindingsModule : BindingModule
    {
        [Header("Scene References")]
        [SerializeField] private GameoverView _gameoverView;

        [Header("SFX")]
        [SerializeField] private AudioEvent _buttonClickEvent;
        [SerializeField] private AudioEvent _gameoverEvent;

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
        private void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement completionManagement, IAudioService audioService)
        {
            _gameoverController.Initialize(levelsCreator, completionManagement, audioService, _buttonClickEvent, _gameoverEvent);
        }
    }
}
