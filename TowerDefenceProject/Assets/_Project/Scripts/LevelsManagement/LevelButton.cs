using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.LevelsManagement
{
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour, IDisposable
    {
        [Header("Scene References")]
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        [SerializeField] private Image _levelImage;
        [SerializeField] private RectTransform _parentForStars;
        [Space]
        [SerializeField] private Image _oneStar;
        [SerializeField] private Image _twoStar;
        [SerializeField] private Image _threeStar;

        [Header("Assets References")]
        [SerializeField] private Sprite _activeStarSprite;
        [SerializeField] private Sprite _inactiveStarSprite;
        [Space]
        [SerializeField] private Sprite _lockedLevelSprite;
        [SerializeField] private Sprite _unlockedLevelSprite;

        private Button _button;
        private int _levelIndex;

        private readonly Subject<int> OnLevelButtonClicked = new();

        public Observable<int> ReadOnlyOnLevelButtonClicked => OnLevelButtonClicked;

        public int LevelIndex => _levelIndex;

        public void Initialize(int levelIndex)
        {
            _levelIndex = levelIndex;
            _button = GetComponent<Button>();

            _button.onClick.AddListener(() => OnLevelButtonClicked?.OnNext(_levelIndex));
            _levelNumberText.text = (_levelIndex + 1).ToString();
        }

        public void Dispose()
        {
            _button.onClick.RemoveAllListeners();
        }

        public void SetActiveLevelView(bool active)
        {
            if (active)
            {
                _parentForStars.gameObject.SetActive(true);
                _levelImage.sprite = _activeStarSprite;
                _levelNumberText.gameObject.SetActive(true);
            }
            else
            {
                _parentForStars.gameObject.SetActive(false);
                _levelImage.sprite = _lockedLevelSprite;
                _levelNumberText.gameObject.SetActive(false);
            }
        }

        public void SetActiveStars(int activeStarsCount)
        {
            _oneStar.sprite = activeStarsCount >= 1 ? _activeStarSprite : _inactiveStarSprite;
            _twoStar.sprite = activeStarsCount >= 2 ? _activeStarSprite : _inactiveStarSprite;
            _threeStar.sprite = activeStarsCount >= 3 ? _activeStarSprite : _inactiveStarSprite;
        }
    }
}
