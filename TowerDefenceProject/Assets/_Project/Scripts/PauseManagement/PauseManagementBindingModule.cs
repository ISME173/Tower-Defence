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
        [SerializeField] private PauseView _pauseView;

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
        private void Initialize(ISaves saves, CameraMoving cameraMoving)
        {
            _pauseController.Initialize(saves, cameraMoving);
        }
    }
}
