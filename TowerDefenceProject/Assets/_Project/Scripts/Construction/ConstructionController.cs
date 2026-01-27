using _Project.Scripts.CameraControll;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private readonly DeferredClickProcessor _deferredClickProcessor;

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

            _pointerPressAction.performed += OnPointerPressPerformed;

            _pointerPositionAction.Enable();
            _pointerPressAction.Enable();

            _deferredClickProcessor = CreateDeferredClickProcessor();
        }

        public void Dispose()
        {
            if (_deferredClickProcessor != null)
                UnityEngine.Object.Destroy(_deferredClickProcessor.gameObject);

            ConstructionView.OnBuildTowerButtonClickedEvent -= OnBuildTowerButtonClicked;
            _pointerPressAction.performed -= OnPointerPressPerformed;

            _pointerPositionAction.Disable();
            _pointerPressAction.Disable();

            _pointerPositionAction.Dispose();
            _pointerPressAction.Dispose();
        }

        private void OnPointerPressPerformed(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.performed == false)
                return;

            _deferredClickProcessor.RequestClick();
        }

        private void ProcessPointerPress()
        {
            if (IsPointerOverUI())
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

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
                return false;

            if (Mouse.current != null)
                return EventSystem.current.IsPointerOverGameObject();

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch != null)
                return EventSystem.current.IsPointerOverGameObject(Touchscreen.current.primaryTouch.touchId.ReadValue());

            return false;
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

        private DeferredClickProcessor CreateDeferredClickProcessor()
        {
            var go = new GameObject(nameof(ConstructionController) + "_" + nameof(DeferredClickProcessor));
            UnityEngine.Object.DontDestroyOnLoad(go);

            var processor = go.AddComponent<DeferredClickProcessor>();
            processor.Initialize(ProcessPointerPress);

            return processor;
        }

        private sealed class DeferredClickProcessor : MonoBehaviour
        {
            private Action _onClickRequested;
            private bool _isClickRequested;

            public void Initialize(Action onClickRequested)
            {
                _onClickRequested = onClickRequested;
            }

            public void RequestClick()
            {
                _isClickRequested = true;
            }

            private void Update()
            {
                if (_isClickRequested == false)
                    return;

                _isClickRequested = false;
                _onClickRequested?.Invoke();
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
