using _Project.Scripts.LevelsManagement;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace _Project.Scripts.CameraControll
{
    [RequireComponent(typeof(Camera))]
    public class CameraMoving : MonoBehaviour
    {
        [Inject] private readonly LevelsCreator LevelsCreator;

        [Header("References")]
        [SerializeField] private Transform _cameraPivot;

        [Header("Rotation")]
        [SerializeField] private float _yawSpeed = 180f;
        [SerializeField] private bool _invertX;
        [SerializeField] private float _rotationAcceleration = 18f;

        [Tooltip("Время (в секундах), за которое скорость вращения затухает почти до 0 после отпускания ввода.")]
        [Min(0.01f)]
        [SerializeField] private float _rotationStopTime = 0.35f;

        [SerializeField] private float _maxYawVelocity = 720f;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 8f;
        [SerializeField] private float _minCameraDistance = 3f;
        [SerializeField] private float _maxCameraDistance = 20f;
        [SerializeField] private float _zoomSmoothTime = 0.12f;

        private InputAction _pointerDeltaAction;

        private InputAction _mouseRightButtonAction;
        private InputAction _touchPressAction;

        private InputAction _mouseScrollAction;
        private InputAction _touch0PositionAction;
        private InputAction _touch1PositionAction;

        private Camera _camera;
        private Transform _cameraTransform;

        private bool _movingIsLocked = false;

        private float _yawVelocity;
        private float _targetCameraDistance;
        private float _zoomVelocity;

        private bool _wasRotatePressed;
        private float _yawStopElapsed;
        private float _yawStopVelocity;

        private CompositeDisposable _compositeDisposables = new CompositeDisposable();

        public Camera Camera => _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            CacheCameraTransform();

            if (_cameraTransform != null && _cameraPivot != null)
            {
                _targetCameraDistance = GetCameraDistance();
            }
        }

        private void OnEnable()
        {
            _pointerDeltaAction = new InputAction(
                name: "PointerDelta",
                type: InputActionType.PassThrough,
                binding: "<Pointer>/delta");

            _mouseRightButtonAction = new InputAction(
                name: "MouseRightButton",
                type: InputActionType.Button,
                binding: "<Mouse>/rightButton");

            _touchPressAction = new InputAction(
                name: "TouchPress",
                type: InputActionType.Button,
                binding: "<Touchscreen>/primaryTouch/press");

            _mouseScrollAction = new InputAction(
                name: "MouseScroll",
                type: InputActionType.PassThrough,
                binding: "<Mouse>/scroll");

            _touch0PositionAction = new InputAction(
                name: "Touch0Position",
                type: InputActionType.PassThrough,
                binding: "<Touchscreen>/touch0/position");

            _touch1PositionAction = new InputAction(
                name: "Touch1Position",
                type: InputActionType.PassThrough,
                binding: "<Touchscreen>/touch1/position");

            _pointerDeltaAction.Enable();
            _mouseRightButtonAction.Enable();
            _touchPressAction.Enable();
            _mouseScrollAction.Enable();
            _touch0PositionAction.Enable();
            _touch1PositionAction.Enable();

            CacheCameraTransform();

            if (_cameraTransform != null && _cameraPivot != null)
            {
                _targetCameraDistance = GetCameraDistance();
                _zoomVelocity = 0f;
            }

            _wasRotatePressed = false;
            _yawStopElapsed = 0f;
            _yawStopVelocity = 0f;

            LevelsCreator.LevelCreated
                .Subscribe(levelObject => OnLevelCreated(levelObject))
                .AddTo(_compositeDisposables);
        }

        private void OnDisable()
        {
            _pointerDeltaAction.Disable();
            _mouseRightButtonAction.Disable();
            _touchPressAction.Disable();
            _mouseScrollAction.Disable();
            _touch0PositionAction.Disable();
            _touch1PositionAction.Disable();

            _pointerDeltaAction.Dispose();
            _mouseRightButtonAction.Dispose();
            _touchPressAction.Dispose();
            _mouseScrollAction.Dispose();
            _touch0PositionAction.Dispose();
            _touch1PositionAction.Dispose();

            _compositeDisposables.Dispose();
        }

        private void Update()
        {
            if (_cameraPivot == null)
                return;

            CacheCameraTransform();

            if (_cameraTransform == null)
                return;

            if (_movingIsLocked)
                return;

            UpdateRotation(Time.deltaTime);
            UpdateZoom(Time.deltaTime);
        }

        public void LockMoving()
        {
            _movingIsLocked = true;
        }

        public void UnlockMoving()
        {
            _movingIsLocked = false;
        }

        private void OnLevelCreated(LevelObject levelObject)
        {
            _cameraPivot = levelObject.CameraPivot;
            transform.SetParent(_cameraPivot);

            _targetCameraDistance = GetCameraDistance();
            _zoomVelocity = 0f;
        }

        private void UpdateRotation(float dt)
        {
            bool rotatePressed = _mouseRightButtonAction.IsPressed() || _touchPressAction.IsPressed();

            float inputYawVelocity = 0f;
            if (rotatePressed)
            {
                Vector2 delta = _pointerDeltaAction.ReadValue<Vector2>();

                float sign = _invertX ? -1f : 1f;
                inputYawVelocity = delta.x * sign * _yawSpeed;

                // Разгон к скорости от ввода
                _yawVelocity = Mathf.Lerp(_yawVelocity, inputYawVelocity, 1f - Mathf.Exp(-_rotationAcceleration * dt));
                _yawVelocity = Mathf.Clamp(_yawVelocity, -_maxYawVelocity, _maxYawVelocity);

                _yawStopElapsed = 0f;
                _yawStopVelocity = 0f;
            }
            else
            {
                // Старт "остановки" ровно в момент отпускания
                if (_wasRotatePressed)
                {
                    _yawStopElapsed = 0f;
                    _yawStopVelocity = _yawVelocity;
                }

                float stopT = Mathf.Max(0.01f, _rotationStopTime);
                _yawStopElapsed += dt;

                float t = Mathf.Clamp01(_yawStopElapsed / stopT);
                _yawVelocity = Mathf.Lerp(_yawStopVelocity, 0f, t);
            }

            _wasRotatePressed = rotatePressed;

            if (Mathf.Abs(_yawVelocity) < 0.001f)
            {
                _yawVelocity = 0f;
                return;
            }

            Vector3 euler = _cameraPivot.eulerAngles;
            euler.y += _yawVelocity * dt;
            _cameraPivot.eulerAngles = euler;
        }

        private void UpdateZoom(float dt)
        {
            float zoomDelta = GetZoomDelta(dt);
            if (Mathf.Abs(zoomDelta) > 0.0001f)
            {
                _targetCameraDistance = Mathf.Clamp(
                    _targetCameraDistance + zoomDelta,
                    _minCameraDistance,
                    _maxCameraDistance);
            }

            float currentDistance = GetCameraDistance();
            float smoothedDistance = Mathf.SmoothDamp(
                currentDistance,
                _targetCameraDistance,
                ref _zoomVelocity,
                _zoomSmoothTime,
                Mathf.Infinity,
                dt);

            SetCameraDistance(smoothedDistance);
        }

        private float GetZoomDelta(float dt)
        {
            Vector2 scroll = _mouseScrollAction.ReadValue<Vector2>();
            if (Mathf.Abs(scroll.y) > 0.01f)
            {
                return -(scroll.y * _zoomSpeed * dt);
            }

            if (Touchscreen.current == null)
            {
                return 0f;
            }

            TouchControl t0 = Touchscreen.current.touches.Count > 0 ? Touchscreen.current.touches[0] : null;
            TouchControl t1 = Touchscreen.current.touches.Count > 1 ? Touchscreen.current.touches[1] : null;

            if (t0 == null || t1 == null)
            {
                return 0f;
            }

            if (!t0.press.isPressed || !t1.press.isPressed)
            {
                return 0f;
            }

            Vector2 p0 = _touch0PositionAction.ReadValue<Vector2>();
            Vector2 p1 = _touch1PositionAction.ReadValue<Vector2>();

            Vector2 p0Prev = p0 - t0.delta.ReadValue();
            Vector2 p1Prev = p1 - t1.delta.ReadValue();

            float prevDist = Vector2.Distance(p0Prev, p1Prev);
            float currDist = Vector2.Distance(p0, p1);

            float pinchDelta = currDist - prevDist;
            return -(pinchDelta * _zoomSpeed * dt);
        }

        private void CacheCameraTransform()
        {
            if (_cameraTransform != null)
            {
                return;
            }

            Camera cam = Camera.main;
            if (cam != null)
            {
                _cameraTransform = cam.transform;
            }
        }

        private float GetCameraDistance()
        {
            return Vector3.Distance(_cameraPivot.position, _cameraTransform.position);
        }

        private void SetCameraDistance(float distance)
        {
            float clamped = Mathf.Clamp(distance, _minCameraDistance, _maxCameraDistance);

            Vector3 dirFromPivotToCamera = (_cameraTransform.position - _cameraPivot.position).normalized;
            _cameraTransform.position = _cameraPivot.position + dirFromPivotToCamera * clamped;
        }
    }
}
