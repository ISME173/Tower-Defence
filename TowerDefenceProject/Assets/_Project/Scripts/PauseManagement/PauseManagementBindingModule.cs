using _Project.Scripts.Advertisement;
using _Project.Scripts.Audio;
using _Project.Scripts.CameraControll;
using _Project.Scripts.DI;
using _Project.Scripts.Saves;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.PauseManagement
{
    public class PauseManagementBindingModule : BindingModule
    {
        [Header("References")]
        [SerializeField] private PauseView _pauseView;

        [Header("SFX")]
        [SerializeField] private AudioEvent _buttonClickAudioEvent;

        private PauseController _pauseController;

        private void OnDestroy()
        {
            _pauseController?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _pauseController = new PauseController(_pauseView);

            containerBuilder.RegisterValue(_pauseController);
        }

        [Inject]
        private void Initialize(ISaves saves, CameraMoving cameraMoving, IAudioService audioService, IAdvertisement advertisement)
        {
            _pauseController.Initialize(saves, cameraMoving, audioService, _buttonClickAudioEvent, advertisement);
        }
    }
}
