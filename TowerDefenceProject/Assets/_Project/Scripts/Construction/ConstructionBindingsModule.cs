using _Project.Scripts.CameraControll;
using _Project.Scripts.DI;
using _Project.Scripts.LevelsManagement;
using _Project.Scripts.MoneySystem;
using _Project.Scripts.PauseManagement;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.Construction
{
    public class ConstructionBindingsModule : BindingModule
    {
        [SerializeField] private ConstructionView _constructionView;
        [SerializeField] private CameraMoving _cameraMoving;
        [SerializeField] private ConstructionController.ConstructionControllerParameters _constructionControllerParameters;

        private ConstructionController _constructionController;

        private void OnDestroy()
        {
            _constructionController?.Dispose();
            _constructionView?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _constructionController = new ConstructionController(_constructionView, _cameraMoving, _constructionControllerParameters);

            containerBuilder.RegisterValue(_constructionController);
            containerBuilder.RegisterValue(_constructionView);
        }

        [Inject]
        private void Initialize(MoneyManagement moneyManagement, LevelCompletionManagement levelCompletionManagement, LevelsCreator levelsCreator, PauseController pauseController)
        {
            _constructionController.Initialize(moneyManagement, levelCompletionManagement, levelsCreator, pauseController);
        }
    }
}
