using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Construction
{
    [RequireComponent(typeof(Button))]
    public class TowerSlotUI : MonoBehaviour
    {
        [SerializeField] private Tower _towerPrefab;

        private Button _button;

        public event Action<Tower> OnBuildTowerButtonClick;

        public Tower TowerPrefab => _towerPrefab;

        public void Initialize()
        {
            _button = GetComponent<Button>();

            _button.onClick.AddListener(() => OnBuildTowerButtonClick?.Invoke(_towerPrefab));
        }
    }
}
