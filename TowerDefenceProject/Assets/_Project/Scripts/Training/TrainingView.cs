using LitMotion;
using UnityEngine;

namespace _Project.Scripts.Training
{

    public class TrainingView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _indexFinger;

        [Header("Click Animation Settings")]
        [SerializeField, Min(0)] private float _clickAnimationDuration;
        [SerializeField] private Ease _clickAnimationEase;

        [Header("Mone Animation Settings")]
        [SerializeField, Min(0)] private float _moveAnimationDuration;
        [SerializeField] private Ease _moveAnimationEase;

        private MotionHandle _currentMotionHandle;

        public void ShowIndexFinger()
        {
            _indexFinger.gameObject.SetActive(true);
        }

        public void HideIndexFinger()
        {
            _indexFinger.gameObject.SetActive(false);
        }
    }
}
