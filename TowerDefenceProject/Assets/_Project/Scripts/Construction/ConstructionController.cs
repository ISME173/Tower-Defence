using _Project.Scripts.CameraControll;
using _Project.Scripts.MoneySystem;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Construction
{
    public class ConstructionController : IDisposable
    {
        private readonly ConstructionView ConstructionView;
        private readonly CameraMoving CameraMoving;
        private readonly CompositeDisposable Disposables = new();

        private ConstructionControllerParameters _constructionControllerParameters;
        private BuildingSite _selectedBuildingSite;
        private DeferredClickProcessor _deferredClickProcessor;
        private MoneyManagement _moneyManagement;

        private InputAction _pointerPositionAction;
        private InputAction _pointerPressAction;

        public ConstructionController(ConstructionView constructionView, CameraMoving cameraMoving, ConstructionControllerParameters constructionControllerParameters)
        {
            ConstructionView = constructionView;
            CameraMoving = cameraMoving;

            ConstructionView.Initialize(constructionControllerParameters.TowerPrefabs);

            ConstructionView.OnBuildTowerButtonClickedEvent += OnBuildTowerButtonClicked;

            ConstructionView.ReadOnlyOnUpgradeCurrentTowerButtonClicked
                .Subscribe(_ => OnUpgradeCurrentTowerButtonClicked())
                .AddTo(Disposables);

            ConstructionView.ReadOnlyOnDestroyCurrentTowerButtonClicled
                .Subscribe(_ => OnDestroyCurrentTowerButtonClicked())
                .AddTo(Disposables);

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

        public void Initialize(MoneyManagement moneyManagement)
        {
            _moneyManagement = moneyManagement;
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

            Disposables.Dispose();
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
                if (hit.collider != null)
                {
                    BuildingSite buildingSite = hit.collider.GetComponent<BuildingSite>() ?? hit.collider.GetComponentInParent<BuildingSite>();

                    if (buildingSite != null)
                    {
                        _selectedBuildingSite?.HideSelectView();
                        _selectedBuildingSite = buildingSite;

                        if (_selectedBuildingSite.CurrentTower == null)
                        {
                            ConstructionView.ShowBuildView();
                        }
                        else if (_selectedBuildingSite.CurrentTower.CanUpgrade())
                        {
                            ConstructionView.UpdateUpgradeTowerView(buildingSite.CurrentTower);
                            ConstructionView.ShowUpgradeView();
                        }

                        _selectedBuildingSite.ShowSelectView();
                        CameraMoving.LockMoving();

                        return;
                    }
                }
            }

            CameraMoving.UnlockMoving();

            _selectedBuildingSite?.HideSelectView();

            ConstructionView.HideBuildView();
            ConstructionView.HideUpgradeView();
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

        private void OnUpgradeCurrentTowerButtonClicked()
        {
            if (_selectedBuildingSite == null)
                return;

            if ((_selectedBuildingSite.CurrentTower != null && _selectedBuildingSite.CurrentTower.CanUpgrade())
                && _moneyManagement.TryGetMoneyForUpgradeTower(_selectedBuildingSite.CurrentTower))
            {
                _selectedBuildingSite.UpgradeCurrentTower();
                _selectedBuildingSite.HideSelectView();

                ConstructionView.HideUpgradeView();
                CameraMoving.UnlockMoving();
            }
        }

        private void OnDestroyCurrentTowerButtonClicked()
        {
            if ((_selectedBuildingSite == null || _selectedBuildingSite.CurrentTower == null)
                || _selectedBuildingSite.CanRemoveCurrentTower() == false)
                return;

            _moneyManagement.OnDestroyedTower(_selectedBuildingSite.CurrentTower);
            _selectedBuildingSite.RemoveCurrentTower();

            _selectedBuildingSite.HideSelectView();

            ConstructionView.HideUpgradeView();
        }

        private void OnBuildTowerButtonClicked(Tower towerPrefab)
        {
            if (_selectedBuildingSite == null)
                return;

            if (_selectedBuildingSite.CanBuildTower() && _moneyManagement.TryGetMoneyForBuildTower(towerPrefab))
            {
                _selectedBuildingSite.BuildTower(towerPrefab);
                _selectedBuildingSite.HideSelectView();

                ConstructionView.HideBuildView();
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
            [Space]
            [SerializeField] private List<Tower> _towerPrefabs;

            public ICollection<Tower> TowerPrefabs => _towerPrefabs;
            public float MaxRaycastMaxDistance => _raycastMaxDistance;
            public LayerMask TargetLayerMask => _targetLayerMask;
        }
    }
}
