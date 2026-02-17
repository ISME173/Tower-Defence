using AnimationsUI.CoreScripts;
using R3;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.VictoryManagement
{
    [RequireComponent(typeof(RectTransform))]
    public class VictoryView : MonoBehaviour, IDisposable
    {
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private PopupAnimationPanelsSequence _panelAnimationSequence;
        [Space]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private readonly Subject<Unit> OnContinueButtonClicked = new(), OnRestartButtonClicked = new(), OnMenuButtonClicked = new();

        public Observable<Unit> ReadOnlyOnContinueButtonClicked => OnContinueButtonClicked;
        public Observable<Unit> ReadOnlyOnRestartButtonClicked => OnRestartButtonClicked;
        public Observable<Unit> ReadOnlyOnMenuButtonClicked => OnMenuButtonClicked;

        public void Initialize()
        {
            _continueButton.onClick.AddListener(() => OnContinueButtonClicked?.OnNext(Unit.Default));
            _restartButton.onClick.AddListener(() => OnRestartButtonClicked?.OnNext(Unit.Default));
            _menuButton.onClick.AddListener(() => OnMenuButtonClicked?.OnNext(Unit.Default));
        }

        public void Dispose()
        {
            _continueButton.onClick.RemoveAllListeners();
            _restartButton.onClick.RemoveAllListeners();
            _menuButton.onClick.RemoveAllListeners();

            OnContinueButtonClicked.Dispose();
            OnRestartButtonClicked.Dispose();
            OnMenuButtonClicked.Dispose();
        }

        public void Show()
        {
            _mainPanel.gameObject.SetActive(true);
            _panelAnimationSequence.Show(null);
        }

        public void Hide()
        {
            //_mainPanel.gameObject.SetActive(false);
            _panelAnimationSequence.Hide(() => _mainPanel.gameObject.SetActive(false));
        }
    }
}
