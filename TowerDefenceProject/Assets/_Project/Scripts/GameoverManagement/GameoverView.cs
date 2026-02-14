using AnimationsUI.CoreScripts;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.GameoverMagamenet
{
    public class GameoverView : MonoBehaviour
    {
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private PopupAnimationPanelsSequence _animationSeqence;
        [Space]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private readonly Subject<Unit> OnRestartButtonClicked = new(), OnMenuButtonClicked = new();

        public Observable<Unit> ReadOnlyOnRestartButtonClicked => OnRestartButtonClicked;
        public Observable<Unit> ReadOnlyOnMenuButtonClicked => OnMenuButtonClicked;

        public void Initialize()
        {
            _restartButton.onClick.AddListener(() => OnRestartButtonClicked?.OnNext(Unit.Default));
            _menuButton.onClick.AddListener(() => OnMenuButtonClicked?.OnNext(Unit.Default));
        }

        public void Dispose()
        {

        }

        public void Show()
        {
            _mainPanel.gameObject.SetActive(true);
            _animationSeqence.Show(null);
        }

        public void Hide()
        {
            _animationSeqence.Hide(() => _mainPanel.gameObject.SetActive(false));
        }
    }
}
