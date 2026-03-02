using _Project.Scripts.Training;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Construction
{
    public class TowerSlotUI : MonoBehaviour, ITrainingStageTrigger
    {
        [SerializeField] private Image _towerIcon;
        [SerializeField] private Image _lockIcon;
        [SerializeField] private TextMeshProUGUI _towerBuildPriceText;
        [SerializeField] private Button _buildTowerButton;

        private readonly Subject<Unit> ReadOnlyTrainingTriggerActivate = new Subject<Unit>();

        public Button BuildTowerButton => _buildTowerButton;
        public Image Icon => _towerIcon;
        public Observable<Unit> OnTrainingTriggerActivate => ReadOnlyTrainingTriggerActivate;

        private void Start()
        {
            BuildTowerButton.onClick.AddListener(InvokeTrainingTrigger);
        }

        private void InvokeTrainingTrigger()
        {
            ReadOnlyTrainingTriggerActivate.OnNext(Unit.Default);
            BuildTowerButton.onClick.RemoveListener(InvokeTrainingTrigger);
        }

        public void UpdateView(Sprite towerIcon, int towerBuildPrice)
        {
            _towerIcon.sprite = towerIcon;
            _towerBuildPriceText.text = towerBuildPrice.ToString();
        }

        public void SetActiveView(bool isActive)
        {
            _towerIcon.gameObject.SetActive(isActive);
            _lockIcon.gameObject.SetActive(!isActive);
            _towerBuildPriceText.gameObject.SetActive(isActive);
            _buildTowerButton.interactable = isActive;
        }
    }
}
