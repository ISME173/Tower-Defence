using AnimationsUI.CoreScripts;
using R3;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.PauseManagement
{
    public class PauseView : MonoBehaviour, IDisposable
    {
        private CompositeDisposable Disposables = new();

        [Header("Scene References")]
        [SerializeField] private RectTransform _viewForHideAndShow;
        [Space]
        [SerializeField] private Image _musicIcon;
        [SerializeField] private Image _sfxIcon;
        [Space]
        [SerializeField] private Button _showPauseButton;
        [SerializeField] private Button _hidePauseButton;
        [SerializeField] private Button _openMenuButton;
        [Space]
        [SerializeField] private Button _musicsActiveSwitchButton;
        [SerializeField] private Button _sfxActiveSwitchButton;
        [Space]
        [SerializeField] private PopupAnimationPanelsSequence _viewAnimationSequence;

        [Header("Assets")]
        [SerializeField] private Sprite _musicActiveSprite;
        [SerializeField] private Sprite _musicInactiveSprite;
        [Space]
        [SerializeField] private Sprite _sfxActiveSprite;
        [SerializeField] private Sprite _sfxInactiveSprite;

        private readonly Subject<Unit> OnShowPauseButtonClicked = new(), OnHidePauseButtonClicked = new(),
            OnMusicsActiveSwitchButtonClicked = new(), OnSfxActiveSwitchButtonClicked = new(), OnOpenMenuButtonClicked = new();

        public Observable<Unit> ReadOnlyOnShowPauseButtonClicked => OnShowPauseButtonClicked;
        public Observable<Unit> ReadOnlyOnHidePauseButtonClicked => OnHidePauseButtonClicked;
        public Observable<Unit> ReadOnlyOnMusicsActiveSwitchButtonClicked => OnMusicsActiveSwitchButtonClicked;
        public Observable<Unit> ReadOnlyOnSfxActiveSwitchButtonClicked => OnSfxActiveSwitchButtonClicked;
        public Observable<Unit> ReadOnlyOnOpenMenuButtonClicked => OnOpenMenuButtonClicked;

        public void Initialize()
        {
            _showPauseButton.OnClickAsObservable().Subscribe(_ => OnShowPauseButtonClicked.OnNext(Unit.Default)).AddTo(Disposables);
            _hidePauseButton.OnClickAsObservable().Subscribe(_ => OnHidePauseButtonClicked.OnNext(Unit.Default)).AddTo(Disposables);
            _musicsActiveSwitchButton.OnClickAsObservable().Subscribe(_ => OnMusicsActiveSwitchButtonClicked.OnNext(Unit.Default)).AddTo(Disposables);
            _sfxActiveSwitchButton.OnClickAsObservable().Subscribe(_ => OnSfxActiveSwitchButtonClicked.OnNext(Unit.Default)).AddTo(Disposables);
            _openMenuButton.OnClickAsObservable().Subscribe(_ => OnOpenMenuButtonClicked.OnNext(Unit.Default)).AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }

        public void Show()
        {
            _viewForHideAndShow.gameObject.SetActive(true);
            _viewAnimationSequence.Show(null);
        }

        public void Hide()
        {
            _viewAnimationSequence.Hide(() => _viewForHideAndShow.gameObject.SetActive(false));
        }

        public void SetActiveMusicView(bool isActive)
        {
            _musicIcon.sprite = isActive ? _musicActiveSprite : _musicInactiveSprite;
        }

        public void SetActiveSoundsView(bool isActive)
        {
            _sfxIcon.sprite = isActive ? _sfxActiveSprite : _sfxInactiveSprite;
        }
    }
}