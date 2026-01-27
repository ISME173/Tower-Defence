using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Construction
{
    public class ConstructionView : MonoBehaviour, IDisposable
    {
        [Header("References")]
        [SerializeField] private GameObject _selectTowerView;
        [Space]
        [SerializeField] private List<TowerSlotUI> _towerSlotUIs;

        public event Action<Tower> OnBuildTowerButtonClickedEvent;

        public bool IsShowedSelectView { get; private set; } = false;

        public void Dispose()
        {
            for (int i = 0; i < _towerSlotUIs.Count; i++)
                _towerSlotUIs[i].OnBuildTowerButtonClick -= OnBuildTowerButtonClicked;
        }

        public void Initialize()
        {
            for (int i = 0; i < _towerSlotUIs.Count; i++)
            {
                _towerSlotUIs[i].Initialize();
                _towerSlotUIs[i].OnBuildTowerButtonClick += OnBuildTowerButtonClicked;
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

        private void OnBuildTowerButtonClicked(Tower tower)
        {
            OnBuildTowerButtonClickedEvent?.Invoke(tower);
        }
    }
}
