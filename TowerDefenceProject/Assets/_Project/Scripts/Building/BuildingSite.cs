using UnityEngine;

namespace _Project.Scripts.Building
{
    [RequireComponent(typeof(Collider))]
    public class BuildingSite : MonoBehaviour
    {
        [SerializeField] private GameObject _view;

        private Collider _collider;
        private Tower _currentTower;

        public Tower CurrentTower => _currentTower;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = false;
        }

        public bool TryBuildTower(Tower towerPrefab)
        {
            if (CurrentTower != null)
                return false;

            _view.SetActive(false);

            Tower newTower = Instantiate(towerPrefab);
            newTower.Initialize();

            newTower.transform.SetParent(transform);
            newTower.transform.localPosition = Vector3.zero;

            return true;
        }

        public bool TryRemoveCurrentTower()
        {
            if (_currentTower == null)
                return false;

            _currentTower.Deinitialize();
            GameObject.Destroy(_currentTower.gameObject);

            _view.SetActive(true);

            return true;
        }
    }
}
