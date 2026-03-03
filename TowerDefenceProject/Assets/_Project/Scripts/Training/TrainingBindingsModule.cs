using _Project.Scripts.CameraControll;
using _Project.Scripts.DI;
using _Project.Scripts.Saves;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.Training
{
    public class TrainingBindingsModule : BindingModule
    {
        [Header("References")]
        [SerializeField] private TrainingView _trainingView;

        [Header("Settings")]
        [SerializeField] private TrainingController.TrainingSettings _trainingSettings;

        private TrainingController _trainingController;

        private void OnDestroy()
        {
            _trainingController?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _trainingController = new TrainingController(_trainingSettings, _trainingView);
            containerBuilder.RegisterValue(_trainingController);
        }

        [Inject]
        private void Initialize(ISaves saves, CameraMoving cameraMoving)
        {
            _trainingController.Initialize(saves, cameraMoving);
        }
    }
}
