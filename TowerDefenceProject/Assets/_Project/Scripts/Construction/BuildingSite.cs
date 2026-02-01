using UnityEngine;

namespace _Project.Scripts.Construction
{
    [RequireComponent(typeof(Collider))]
    public class BuildingSite : MonoBehaviour
    {
        [SerializeField] private GameObject _selectView;
        [SerializeField] private GameObject _mainView;

        private Collider _collider;
        private Tower _currentTower;

        public Tower CurrentTower => _currentTower;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = false;
        }

        public void ShowSelectView()
        {
            _selectView.SetActive(true);
        }

        public void HideSelectView()
        {
            _selectView.SetActive(false);
        }

        public bool CanBuildTower()
        {
            return _currentTower == null;
        }

        public void UpgradeCurrentTower()
        {
            if (_currentTower == null)
            {
                Debug.LogError("Tower is not placed!");
                return;
            }

            if (_currentTower.CanUpgrade() == false)
            {
                Debug.LogWarning("Cannot upgrade current tower!");
                return;
            }

            _mainView.SetActive(false);

            _currentTower.Upgrade();
        }

        public void BuildTower(Tower towerPrefab)
        {
            if (CanBuildTower() == false)
            {
                Debug.LogError($"You can't build a tower!");
                return;
            }

            _mainView.SetActive(false);

            Tower newTower = Instantiate(towerPrefab);
            newTower.Initialize();

            newTower.transform.SetParent(transform);
            newTower.transform.localPosition = Vector3.zero;
        }

        public bool TryRemoveCurrentTower()
        {
            if (_currentTower == null)
                return false;

            _currentTower.Deinitialize();
            GameObject.Destroy(_currentTower.gameObject);

            _mainView.SetActive(true);

            return true;
        }
    }
}
