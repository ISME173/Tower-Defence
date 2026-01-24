using _Project.Scripts.CameraControll;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Construction
{
    public class ConstructionController : IDisposable
    {
        private readonly ConstructionView ConstructionView;
        private readonly CameraMoving CameraMoving;

        private ConstructionControllerParameters _constructionControllerParameters;
        private BuildingSite _selectedBuildingSite;

        private InputAction _pointerPositionAction;
        private InputAction _pointerPressAction;

        public ConstructionController(ConstructionView constructionView, CameraMoving cameraMoving, ConstructionControllerParameters constructionControllerParameters)
        {
            ConstructionView = constructionView;
            CameraMoving = cameraMoving;

            ConstructionView.OnBuildTowerButtonClickedEvent += OnBuildTowerButtonClicked;

            _constructionControllerParameters = constructionControllerParameters;

            _pointerPositionAction = new InputAction(
                name: "PointerPosition",
                type: InputActionType.PassThrough,
                binding: "<Pointer>/position");

            _pointerPressAction = new InputAction(
                name: "PointerPress",
                type: InputActionType.Button,
                binding: "<Pointer>/press");

            _pointerPressAction.performed += TrySearchBuildingSite;

            _pointerPositionAction.Enable();
            _pointerPressAction.Enable();
        }

        public void Dispose()
        {
            ConstructionView.OnBuildTowerButtonClickedEvent -= OnBuildTowerButtonClicked;
            _pointerPressAction.performed -= TrySearchBuildingSite;

            _pointerPositionAction.Disable();
            _pointerPressAction.Disable();

            _pointerPositionAction.Dispose();
            _pointerPressAction.Dispose();
        }

        public void TrySearchBuildingSite(InputAction.CallbackContext callbackContext)
        {
            if (ConstructionView.IsShowedSelectView)
                return;

            Camera cam = CameraMoving.Camera;

            Vector2 screenPos = _pointerPositionAction.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(screenPos);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _constructionControllerParameters.MaxRaycastMaxDistance, _constructionControllerParameters.TargetLayerMask))
            {
                if (hit.collider != null && hit.collider.TryGetComponent(out BuildingSite buildingSite))
                {
                    _selectedBuildingSite = buildingSite;

                    ConstructionView.ShowSelectView();
                    CameraMoving.LockMoving();

                    return;
                }
            }

            CameraMoving.UnlockMoving();
            ConstructionView.HideSelectView();
        }

        private void OnBuildTowerButtonClicked(Tower towerPrefab)
        {
            if (_selectedBuildingSite == null)
                return;

            if (_selectedBuildingSite.TryBuildTower(towerPrefab))
            {
                ConstructionView.HideSelectView();
                CameraMoving.UnlockMoving();
            }
        }

        [Serializable]
        public struct ConstructionControllerParameters
        {
            [SerializeField, Min(0)] private float _raycastMaxDistance;
            [SerializeField] private LayerMask _targetLayerMask;

            public float MaxRaycastMaxDistance => _raycastMaxDistance;
            public LayerMask TargetLayerMask => _targetLayerMask;
        }
    }
}
