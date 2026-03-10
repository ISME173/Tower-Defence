using _Project.Scripts.Training;
using R3;
using UnityEngine;

namespace _Project.Scripts.Construction
{
    [RequireComponent(typeof(Collider))]
    public class BuildingSite : MonoBehaviour, ITrainingStageTrigger, IDisableDuringTraining
    {
        [SerializeField] private GameObject _selectView;
        [SerializeField] private GameObject _mainView;
        [SerializeField] private ParticleSystem _buildEffect;

        private Collider _collider;
        private Tower _currentTower;

        private readonly Subject<Unit> ReadOnlyOnTrainingTriggerActivate = new Subject<Unit>();

        public Tower CurrentTower => _currentTower;
        public Observable<Unit> OnTrainingTriggerActivate => ReadOnlyOnTrainingTriggerActivate;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = false;
        }

        public void ShowSelectView()
        {
            if (_selectView != null)
                _selectView.SetActive(true);

            ReadOnlyOnTrainingTriggerActivate.OnNext(Unit.Default);
        }

        public void HideSelectView()
        {
            if (_selectView != null)
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

            _buildEffect.Play();

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

            _buildEffect.Play();

            _mainView.SetActive(false);

            Tower newTower = Instantiate(towerPrefab);
            newTower.Initialize();

            newTower.transform.SetParent(transform);
            newTower.transform.localPosition = Vector3.zero;

            _currentTower = newTower;
        }

        public bool CanRemoveCurrentTower()
        {
            if (_currentTower == null)
                return false;

            return true;
        }

        public void RemoveCurrentTower()
        {
            if (CanRemoveCurrentTower() == false)
            {
                Debug.LogError($"You can't remove a tower!");
                return;
            }

            _buildEffect.Play();

            _currentTower.Dispose();
            GameObject.Destroy(_currentTower.gameObject);
            _currentTower = null;

            _mainView.SetActive(true);
        }

        public void Disable()
        {
            Collider collider = _collider ?? GetComponent<Collider>();
            collider.enabled = false;
        }

        public void Enable()
        {
            Collider collider = _collider ?? GetComponent<Collider>();
            collider.enabled = true;
        }
    }
}
