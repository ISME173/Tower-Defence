using _Project.Scripts.Audio;
using _Project.Scripts.DI;
using _Project.Scripts.LevelsManagement;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.VictoryManagement
{

    public class VictoryManagementBindingsModule : BindingModule
    {
        [Header("Scene References")]
        [SerializeField] private VictoryView _victoryView;

        [Header("Audio")]
        [SerializeField] private AudioEvent _buttonClickAudioEvent;

        private VictoryController _victoryController;

        private void OnDestroy()
        {
            _victoryController?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _victoryController = new VictoryController(_victoryView);
            containerBuilder.RegisterValue(_victoryController);
        }

        [Inject]
        private void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement, IAudioService audioService)
        {
            _victoryController.Initialize(levelsCreator, levelCompletionManagement, audioService, _buttonClickAudioEvent);
        }
    }
}
