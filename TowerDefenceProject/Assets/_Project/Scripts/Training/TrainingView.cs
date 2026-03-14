using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Training
{
    public class TrainingView : MonoBehaviour
    {
        [Header("References")]
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

    }
}
