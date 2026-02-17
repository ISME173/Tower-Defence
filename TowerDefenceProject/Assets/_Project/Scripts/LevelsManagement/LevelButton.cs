using R3;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.LevelsManagement
{
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour, IDisposable
    {
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
        }

        public void Dispose()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}
