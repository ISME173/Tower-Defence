using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelsListView : MonoBehaviour, IDisposable
    {
        private readonly List<LevelsPanel> LevelsPanels = new();
        private readonly CompositeDisposable Disposables = new();

        [Header("References")]
        [SerializeField] private Button _openNextLevelsPanelButton;
        [SerializeField] private Button _openPreviousLevelsPanelButton;
        [Space]
        [SerializeField] private Transform _parentForLevelsPanels;
        [SerializeField] private Transform _pointForNextLevelsPanel;
        [SerializeField] private Transform _pointForPreviousLevelsPanel;
        [Space]
        [SerializeField] private RectTransform _viewForHideAndShow;
        [Space]
        [SerializeField] private LevelsPanel _levelsPanelPrefab;

        private int _currentLevelsPanelIndex = 0;
        private LevelsPanel _currentLevelsPanel;

        private readonly Subject<int> OnLevelButtonClicked = new();
        private readonly Subject<Unit> OnNextLevelsPanelButtonClicked = new(), OnPreviousLevelsPanelButtonClicked = new();

        public Observable<Unit> ReadOnlyOnNextLevelsPanelButtonClicked => OnNextLevelsPanelButtonClicked;
        public Observable<Unit> ReadOnlyOnPreviousLevelsPanelButtonClicked => OnPreviousLevelsPanelButtonClicked;
        public Observable<int> ReadOnlyOnLevelButtonClicked => OnLevelButtonClicked;

        public int CurrentLevelsPanelIndex => _currentLevelsPanelIndex;
        public int LevelsPanelsCount => LevelsPanels.Count;

        public void Initialize(int levelsCount)
        {
            ClearLevelsPanels();

            _currentLevelsPanelIndex = 0;
            _currentLevelsPanel = null;

            _openNextLevelsPanelButton.onClick.RemoveAllListeners();
            _openPreviousLevelsPanelButton.onClick.RemoveAllListeners();

            _openNextLevelsPanelButton.onClick.AddListener(() => OnNextLevelsPanelButtonClicked?.OnNext(Unit.Default));
            _openPreviousLevelsPanelButton.onClick.AddListener(() => OnPreviousLevelsPanelButtonClicked?.OnNext(Unit.Default));

            if (levelsCount <= 0)
                return;

            int maxButtonsInPanel = _levelsPanelPrefab.MaxLevelButtonsInPanel;
            int panelsCount = (levelsCount + maxButtonsInPanel - 1) / maxButtonsInPanel; // ceil

            for (int panelNumber = 1; panelNumber <= panelsCount; panelNumber++)
            {
                LevelsPanel newLevelsPanel = Instantiate(_levelsPanelPrefab, _parentForLevelsPanels);

                newLevelsPanel.Initialize(
                    CalculateLevelIndexesInPanel(panelNumber, levelsCount, newLevelsPanel.MaxLevelButtonsInPanel));

                newLevelsPanel.ReadOnlyOnLevelButtonClicked
                    .Subscribe(clickedLevelIndex => OnLevelButtonClicked?.OnNext(clickedLevelIndex))
                    .AddTo(Disposables);

                LevelsPanels.Add(newLevelsPanel);
                newLevelsPanel.gameObject.SetActive(false);
            }

            if (LevelsPanels.Count > 0)
            {
                _currentLevelsPanel = LevelsPanels[_currentLevelsPanelIndex];
                _currentLevelsPanel.gameObject.SetActive(true);
                _currentLevelsPanel.transform.localPosition = Vector3.zero;
            }
        }

        public void Dispose()
        {
            ClearLevelsPanels();
            Disposables.Dispose();
        }

        public void ShowView()
        {
            _viewForHideAndShow.gameObject.SetActive(true);
        }

        public void HideView()
        {
            _viewForHideAndShow.gameObject.SetActive(false);
        }

        public void OpenNextLevelsPanel()
        {
            if (_currentLevelsPanelIndex + 1 >= LevelsPanels.Count)
                throw new ArgumentOutOfRangeException();

            _currentLevelsPanelIndex++;

            _currentLevelsPanel.transform.SetParent(_pointForNextLevelsPanel, false);
            _currentLevelsPanel.transform.localPosition = Vector3.zero; 
            _currentLevelsPanel?.gameObject.SetActive(false);

            _currentLevelsPanel = LevelsPanels[_currentLevelsPanelIndex];

            _currentLevelsPanel.transform.SetParent(_parentForLevelsPanels, false);
            _currentLevelsPanel.transform.localPosition = Vector3.zero;
            _currentLevelsPanel.gameObject.SetActive(true);
        }

        public void OpenPreviousLevelsPanel()
        {
            if (_currentLevelsPanelIndex - 1 < 0)
                throw new ArgumentOutOfRangeException();

            _currentLevelsPanelIndex--;

            _currentLevelsPanel.transform.SetParent(_pointForPreviousLevelsPanel, false);
            _currentLevelsPanel.transform.localPosition = Vector3.zero;
            _currentLevelsPanel?.gameObject.SetActive(false);

            _currentLevelsPanel = LevelsPanels[_currentLevelsPanelIndex];

            _currentLevelsPanel.transform.SetParent(_parentForLevelsPanels, false);
            _currentLevelsPanel.transform.localPosition = Vector3.zero;
            _currentLevelsPanel.gameObject.SetActive(true);
        }

        public void HideOpenPreviousLevelsPanelButton() => _openPreviousLevelsPanelButton.gameObject.SetActive(false);
        public void ShowOpenPreviousLevelsPanelButton() => _openPreviousLevelsPanelButton.gameObject.SetActive(true);

        public void HideOpenNextLevelsPanelButton() => _openNextLevelsPanelButton.gameObject.SetActive(false);
        public void ShowOpenNextLevelsPanelButton() => _openNextLevelsPanelButton.gameObject.SetActive(true);

        private void ClearLevelsPanels()
        {
            if (LevelsPanels.Count == 0)
                return;

            foreach (var levelsPanel in LevelsPanels)
            {
                levelsPanel.Dispose();
                Destroy(levelsPanel.gameObject);
            }

            LevelsPanels.Clear();
        }

        private int[] CalculateLevelIndexesInPanel(int panelNumber, int totalLevelsCount, int maxLevelButtonsCountInPanel)
        {
            if (panelNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(panelNumber));

            if (totalLevelsCount < 0)
                throw new ArgumentOutOfRangeException(nameof(totalLevelsCount));

            if (maxLevelButtonsCountInPanel < 1)
                throw new ArgumentOutOfRangeException(nameof(maxLevelButtonsCountInPanel));

            if (totalLevelsCount == 0)
                return Array.Empty<int>();

            int startIndex = (panelNumber - 1) * maxLevelButtonsCountInPanel;

            if (startIndex >= totalLevelsCount)
                return Array.Empty<int>();

            int count = Math.Min(maxLevelButtonsCountInPanel, totalLevelsCount - startIndex);

            var result = new int[count];
            for (int i = 0; i < count; i++)
                result[i] = startIndex + i;

            return result;
        }
    }
}
