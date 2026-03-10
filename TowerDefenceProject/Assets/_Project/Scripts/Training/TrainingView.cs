using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Training
{
    public class TrainingView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _indexFinger;
        [SerializeField] private RectTransform _mainInfoPanel;
        [SerializeField] private TextMeshProUGUI _infoText;

        [Header("Click Animation Settings")]
        [SerializeField, Min(0)] private float _clickAnimationDuration;
        [SerializeField] private Ease _clickAnimationEase;
        [Space]
        [SerializeField] private Vector3 _startClickAnimation;
        [SerializeField] private Vector3 _endClickAnimation;

        [Header("Mone Animation Settings")]
        [SerializeField, Min(0)] private float _moveAnimationDuration;
        [SerializeField] private Ease _moveAnimationEase;

        private MotionHandle _currentMotionHandle;

        public Vector3 IndexFingerPosition => _indexFinger.position;

        public void ShowIndexFinger()
        {
            _indexFinger.gameObject.SetActive(true);
        }

        public void HideIndexFinger()
        {
            _indexFinger.gameObject.SetActive(false);
        }

        public void ShowInfoPanel()
        {
            _mainInfoPanel.gameObject.SetActive(true);
        }

        public void HideInfoPanel()
        {
            _mainInfoPanel.gameObject.SetActive(false);
        }

        public void SetInfoText(string text)
        {
            _infoText.text = text;
        }

        public void StopCurrentAnimation()
        {
            _currentMotionHandle.TryCancel();
        }

        public void MoveIndexFinger(Vector3 from, Vector3 to)
        {
            _indexFinger.transform.position = from;

            _currentMotionHandle.TryCancel();

            _currentMotionHandle = LMotion.Create(from, to, _moveAnimationDuration)
                .WithEase(_moveAnimationEase)
                .BindToPosition(_indexFinger);
        }

        public void IndexFingerClickAnimationStart()
        {
            _currentMotionHandle.TryCancel();

            _currentMotionHandle = LMotion.Create(_startClickAnimation, _endClickAnimation, _clickAnimationDuration)
                .WithEase(_clickAnimationEase)
                .WithLoops(-1, LoopType.Restart)
                .BindToPosition(_indexFinger);
        }
    }
}
