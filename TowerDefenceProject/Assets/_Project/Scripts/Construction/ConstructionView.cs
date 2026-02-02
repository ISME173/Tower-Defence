using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Construction
{
    public class ConstructionView : MonoBehaviour, IDisposable
    {
        private readonly Dictionary<TowerSlotUI, Tower> TowerSlotUIsWithTowerPrefabs = new();

        [Header("References")]
        [SerializeField] private GameObject _selectTowerView;
        [Space]
        [SerializeField] private List<TowerSlotUI> _towerSlotUIs;

        public event Action<Tower> OnBuildTowerButtonClickedEvent;

        public bool IsShowedSelectView { get; private set; } = false;

        public void Dispose()
        {
            for (int i = 0; i < _towerSlotUIs.Count; i++)
                _towerSlotUIs[i].BuildTowerButton.onClick.RemoveAllListeners();
        }

        public void Initialize(ICollection<Tower> towerPrefabs)
        {
            if (towerPrefabs.Count > _towerSlotUIs.Count)
            {
                Debug.LogError("Tower prefabs count > tower slot UIs count");
                return;
            }

            TowerSlotUIsWithTowerPrefabs.Clear();

            for (int i = 0; i < _towerSlotUIs.Count; i++)
                _towerSlotUIs[i].gameObject.SetActive(false);

            int currentTowerSlotUiIndex = 0;
            foreach (var towerPrefab in towerPrefabs)
            {
                TowerSlotUI towerSlotUI = _towerSlotUIs[currentTowerSlotUiIndex];
                towerSlotUI.UpdateView(towerPrefab.TowerIconSprite, towerPrefab.BuildPrice);
                towerSlotUI.gameObject.SetActive(true);

                towerSlotUI.BuildTowerButton.onClick.AddListener(() =>
                {
                    OnBuildTowerButtonClickedEvent?.Invoke(towerPrefab);
                });

                TowerSlotUIsWithTowerPrefabs.Add(towerSlotUI, towerPrefab);
                currentTowerSlotUiIndex++;
            }
        }

        public void UpdateTowerUiViews(ICollection<Tower> towerPrefabs)
        {
            if (towerPrefabs.Count > _towerSlotUIs.Count)
            {
                Debug.LogError("Tower prefabs count > tower slot UIs count");
                return;
            }

            for (int i = 0; i < _towerSlotUIs.Count; i++)
                _towerSlotUIs[i].gameObject.SetActive(false);

            int currentTowerSlotUiIndex = 0;
            foreach (var towerPrefab in towerPrefabs)
            {
                TowerSlotUI towerSlotUI = _towerSlotUIs[currentTowerSlotUiIndex];
                towerSlotUI.UpdateView(towerPrefab.GetTowerIconForNextLevel(), towerPrefab.GetBuildPriceForNextLevel());
                towerSlotUI.gameObject.SetActive(true);

                currentTowerSlotUiIndex++;
            }
        }

        public void ShowSelectView()
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                var mousePosition = mouse.position.ReadValue();
                _selectTowerView.transform.position = new Vector3(mousePosition.x, mousePosition.y, _selectTowerView.transform.position.z);
            }

            _selectTowerView.SetActive(true);
            IsShowedSelectView = true;
        }

        public void HideSelectView()
        {
            _selectTowerView.SetActive(false);
            IsShowedSelectView = false;
        }
    }
}
