using _Project.Scripts.Utilities;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.LevelsManagement
{
    public class LevelsPanel : MonoBehaviour, IDisposable
    {
        private readonly HashSet<LevelButton> LevelButtonsInPanel = new();
        private readonly CompositeDisposable Disposables = new();

        [Header("References")]
        [SerializeField] private LevelButton _levelButtonPrefab;
        [SerializeField] private GridLayoutGroup _parentForLevelButtons;

        [Header("Settings")]
        [SerializeField, Min(1)] private int _maxLevelButtonsInPanel = 10;

        private ObjectPoolWithQueue<LevelButton> _levelButtonsPool;

        private readonly Subject<int> OnLevelButtonClicked = new();

        public Observable<int> ReadOnlyOnLevelButtonClicked => OnLevelButtonClicked;
        public int MaxLevelButtonsInPanel => _maxLevelButtonsInPanel;

        public void Initialize(int[] levelIndexesInPanel)
        {
            ClearCurrentLevelButtons();

            _levelButtonsPool = new ObjectPoolWithQueue<LevelButton>(_levelButtonPrefab, _parentForLevelButtons.transform);

            for (int i = 0; i < levelIndexesInPanel.Length; i++)
            {
                LevelButton newLevelButton = _levelButtonsPool.GetObject();
                newLevelButton.transform.SetParent(_parentForLevelButtons.transform, false);
                newLevelButton.Initialize(levelIndexesInPanel[i]);
                newLevelButton.ReadOnlyOnLevelButtonClicked
                    .Subscribe(levelIndex => OnLevelButtonClicked?.OnNext(levelIndex))
                    .AddTo(Disposables);

                LevelButtonsInPanel.Add(newLevelButton);
            }
        }

        public void UpdateButtons(Func<int, bool> isUnlocked, Func<int, int> getStars)
        {
            if (isUnlocked == null)
                throw new ArgumentNullException(nameof(isUnlocked));
            if (getStars == null)
                throw new ArgumentNullException(nameof(getStars));

            foreach (var button in LevelButtonsInPanel)
            {
                int levelIndex = button.LevelIndex;

                bool unlocked = isUnlocked(levelIndex);
                button.SetActiveLevelView(unlocked);

                int stars = unlocked ? getStars(levelIndex) : 0;
                button.SetActiveStars(stars);
            }
        }

        public void Dispose()
        {
            ClearCurrentLevelButtons();
            Disposables.Dispose();
        }

        private void ClearCurrentLevelButtons()
        {
            if (LevelButtonsInPanel.Count == 0)
                return;

            foreach (var levelButton in LevelButtonsInPanel)
            {
                levelButton.Dispose();
                _levelButtonsPool.AddObject(levelButton);
            }

            LevelButtonsInPanel.Clear();
        }
    }
}
