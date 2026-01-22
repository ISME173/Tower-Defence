using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace _Project.Scripts.CameraControll
{
    public class CameraMoving : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _cameraPivot;

        [Header("Rotation")]
        [SerializeField] private float _yawSpeed = 180f;
        [SerializeField] private bool _invertX;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 8f;
        [SerializeField] private float _minCameraDistance = 3f;
        [SerializeField] private float _maxCameraDistance = 20f;

        private InputAction _pointerDeltaAction;

        private InputAction _mouseRightButtonAction;
        private InputAction _touchPressAction;

        private InputAction _mouseScrollAction;
        private InputAction _touch0PositionAction;
        private InputAction _touch1PositionAction;

        private Transform _cameraTransform;

        private void Awake()
        {
            CacheCameraTransform();
        }

        private void OnEnable()
        {
            // Общие
            _pointerDeltaAction = new InputAction(
                name: "PointerDelta",
                type: InputActionType.PassThrough,
                binding: "<Pointer>/delta");

            // ПК: вращение по ПКМ
            _mouseRightButtonAction = new InputAction(
                name: "MouseRightButton",
                type: InputActionType.Button,
                binding: "<Mouse>/rightButton");

            // Мобилка: вращение/жесты по удержанию пальца
            _touchPressAction = new InputAction(
                name: "TouchPress",
                type: InputActionType.Button,
                binding: "<Touchscreen>/primaryTouch/press");

            // ПК: зум колесом
            _mouseScrollAction = new InputAction(
                name: "MouseScroll",
                type: InputActionType.PassThrough,
                binding: "<Mouse>/scroll");

            // Мобилка: pinch по позициям двух касаний
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
        }

        private void Update()
        {
            if (_cameraPivot == null)
            {
                return;
            }

            CacheCameraTransform();

            if (_cameraTransform == null)
            {
                return;
            }

            UpdateRotation();
            UpdateZoom();
        }

        private void UpdateRotation()
        {
            // ПК: только при зажатой ПКМ
            // Мобильное: только при удержании пальца
            bool rotatePressed = _mouseRightButtonAction.IsPressed() || _touchPressAction.IsPressed();
            if (!rotatePressed)
            {
                return;
            }

            Vector2 delta = _pointerDeltaAction.ReadValue<Vector2>();

            float sign = _invertX ? -1f : 1f;
            float yawDelta = delta.x * sign * _yawSpeed * Time.deltaTime;

            Vector3 euler = _cameraPivot.eulerAngles;
            euler.y += yawDelta;
            _cameraPivot.eulerAngles = euler;
        }

        private void UpdateZoom()
        {
            // ПК: колесо мыши (Y) -> приближение/отдаление
            Vector2 scroll = _mouseScrollAction.ReadValue<Vector2>();

            float currentDistance = 0;
            float nextDistance = 0;

            if (Mathf.Abs(scroll.y) > 0.01f)
            {
                currentDistance = GetCameraDistance();
                nextDistance = currentDistance - (scroll.y * _zoomSpeed * Time.deltaTime);
                SetCameraDistance(nextDistance);
                return;
            }

            // Мобилка: pinch (2 пальца)
            if (Touchscreen.current == null)
            {
                return;
            }

            TouchControl t0 = Touchscreen.current.touches.Count > 0 ? Touchscreen.current.touches[0] : null;
            TouchControl t1 = Touchscreen.current.touches.Count > 1 ? Touchscreen.current.touches[1] : null;

            if (t0 == null || t1 == null)
            {
                return;
            }

            if (!t0.press.isPressed || !t1.press.isPressed)
            {
                return;
            }

            Vector2 p0 = _touch0PositionAction.ReadValue<Vector2>();
            Vector2 p1 = _touch1PositionAction.ReadValue<Vector2>();

            Vector2 p0Prev = p0 - t0.delta.ReadValue();
            Vector2 p1Prev = p1 - t1.delta.ReadValue();

            float prevDist = Vector2.Distance(p0Prev, p1Prev);
            float currDist = Vector2.Distance(p0, p1);

            float pinchDelta = currDist - prevDist;

            // Пальцы расходятся -> pinchDelta > 0 -> приближаем (уменьшаем дистанцию)
            currentDistance = GetCameraDistance();
            nextDistance = currentDistance - (pinchDelta * _zoomSpeed * Time.deltaTime);
            SetCameraDistance(nextDistance);
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

            // Двигаем камеру вдоль её forward к pivot (по линии камера <-> pivot)
            Vector3 dirFromPivotToCamera = (_cameraTransform.position - _cameraPivot.position).normalized;
            _cameraTransform.position = _cameraPivot.position + dirFromPivotToCamera * clamped;
        }
    }
}
