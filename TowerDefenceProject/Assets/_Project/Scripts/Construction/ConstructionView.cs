using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Scripts.Construction
{
    public class ConstructionView : MonoBehaviour, IDisposable
    {
        private readonly Dictionary<TowerSlotUI, Tower> TowerSlotUIsWithTowerPrefabs = new();

        [Header("References")]
        [SerializeField] private GameObject _buildTowerView;
        [SerializeField] private List<TowerSlotUI> _towerSlotsInBuildTowerView;
        [Space]
        [SerializeField] private GameObject _upgradeTowerView;
        [SerializeField] private TowerSlotUI _upgradeTowerSlot;
        [Space]
        [SerializeField] private Button _destroyTowerButtom;

        public event Action<Tower> OnBuildTowerButtonClickedEvent;

        private readonly Subject<Unit> OnUpgradeCurrentTowerButtonClicked = new();
        private readonly Subject<Unit> OnDestroyCurrentTowerButtonClicked = new();

        public Observable<Unit> ReadOnlyOnUpgradeCurrentTowerButtonClicked => OnUpgradeCurrentTowerButtonClicked;
        public Observable<Unit> ReadOnlyOnDestroyCurrentTowerButtonClicled => OnDestroyCurrentTowerButtonClicked;

        public bool IsShowedSelectView { get; private set; } = false;

        public void Dispose()
        {
            for (int i = 0; i < _towerSlotsInBuildTowerView.Count; i++)
                _towerSlotsInBuildTowerView[i].BuildTowerButton.onClick.RemoveAllListeners();

            _upgradeTowerSlot.BuildTowerButton.onClick.RemoveAllListeners();
        }

        public void UpdateCanPlaceTowersList(ICollection<Tower> towerPrefabsCanPlace)
        {
            if (towerPrefabsCanPlace.Count > _towerSlotsInBuildTowerView.Count)
            {
                Debug.LogError("Tower prefabs count > tower slot UIs count");
                return;
            }
            List<Tower> towerPrefabsCanPlaceList = new(towerPrefabsCanPlace);

            TowerSlotUIsWithTowerPrefabs.Clear();

            for (int i = 0; i < _towerSlotsInBuildTowerView.Count; i++)
                _towerSlotsInBuildTowerView[i].gameObject.SetActive(false);

            for (int i = 0; i < _towerSlotsInBuildTowerView.Count; i++)
            {
                TowerSlotUI towerSlotUI = _towerSlotsInBuildTowerView[i];
                Tower towerPrefab = towerPrefabsCanPlaceList.Count > i ? towerPrefabsCanPlaceList[i] : null;

                towerSlotUI.gameObject.SetActive(true);

                if (towerPrefab != null)
                {
                    towerSlotUI.UpdateView(towerPrefab.TowerIconSprite, towerPrefab.BuildPrice);
                    towerSlotUI.SetActiveView(true);
                }
                else
                {
                    towerSlotUI.SetActiveView(false);
                }
            }

            int currentTowerSlotUiIndex = 0;
            foreach (var towerPrefab in towerPrefabsCanPlace)
            {
                TowerSlotUI towerSlotUI = _towerSlotsInBuildTowerView[currentTowerSlotUiIndex];
                towerSlotUI.UpdateView(towerPrefab.TowerIconSprite, towerPrefab.BuildPrice);
                towerSlotUI.gameObject.SetActive(true);

                towerSlotUI.BuildTowerButton.onClick.RemoveAllListeners();
                towerSlotUI.BuildTowerButton.onClick.AddListener(() =>
                {
                    OnBuildTowerButtonClickedEvent?.Invoke(towerPrefab);
                });

                TowerSlotUIsWithTowerPrefabs.Add(towerSlotUI, towerPrefab);
                currentTowerSlotUiIndex++;
            }
        }

        public void Initialize()
        {
            _upgradeTowerSlot.BuildTowerButton.onClick.AddListener(() =>
            {
                OnUpgradeCurrentTowerButtonClicked?.OnNext(Unit.Default);
            });

            _destroyTowerButtom.onClick.AddListener(() =>
            {
                OnDestroyCurrentTowerButtonClicked?.OnNext(Unit.Default);
            });
        }

        public void UpdateUpgradeTowerView(Tower towerForUpgrade)
        {
            if (towerForUpgrade.CanUpgrade())
            {
                _upgradeTowerSlot.gameObject.SetActive(true);
                _upgradeTowerSlot.UpdateView(_upgradeTowerSlot.Icon.sprite, towerForUpgrade.GetBuildPriceForNextLevel());
            }
            else
                _upgradeTowerSlot.gameObject.SetActive(false);
        }

        public void UpdateBuildTowerView(ICollection<Tower> towerPrefabs)
        {
            if (towerPrefabs.Count > _towerSlotsInBuildTowerView.Count)
            {
                Debug.LogError("Tower prefabs count > tower slot UIs count");
                return;
            }

            for (int i = 0; i < _towerSlotsInBuildTowerView.Count; i++)
                _towerSlotsInBuildTowerView[i].gameObject.SetActive(false);

            int currentTowerSlotUiIndex = 0;
            foreach (var towerPrefab in towerPrefabs)
            {
                TowerSlotUI towerSlotUI = _towerSlotsInBuildTowerView[currentTowerSlotUiIndex];
                towerSlotUI.UpdateView(towerPrefab.GetTowerIconForNextLevel(), towerPrefab.GetBuildPriceForNextLevel());
                towerSlotUI.gameObject.SetActive(true);

                currentTowerSlotUiIndex++;
            }
        }

        public void ShowUpgradeView(Vector2 pointerPosition)
        {
            _upgradeTowerView.transform.position = new Vector3(
                pointerPosition.x,
                pointerPosition.y,
                _upgradeTowerView.transform.position.z);

            _upgradeTowerView.SetActive(true);
        }

        public void HideUpgradeView()
        {
            _upgradeTowerView.SetActive(false);
        }

        public void ShowBuildView(Vector2 pointerPosition)
        {
            _buildTowerView.transform.position = new Vector3(
                pointerPosition.x,
                pointerPosition.y,
                _buildTowerView.transform.position.z);

            _buildTowerView.SetActive(true);
            IsShowedSelectView = true;
        }

        public void HideBuildView()
        {
            _buildTowerView.SetActive(false);
            IsShowedSelectView = false;
        }

        private bool TryGetPointerPosition(out Vector2 pointerPosition)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                pointerPosition = touchscreen.primaryTouch.position.ReadValue();
                return true;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                pointerPosition = mouse.position.ReadValue();
                return true;
            }

            pointerPosition = default;
            return false;
        }
    }
}
