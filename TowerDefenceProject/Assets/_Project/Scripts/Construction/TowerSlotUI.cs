using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Construction
{
    public class TowerSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _towerIcon;
        [SerializeField] private TextMeshProUGUI _towerBuildPriceText;
        [SerializeField] private Button _buildTowerButton;

        public Button BuildTowerButton => _buildTowerButton;
        public Image Icon => _towerIcon;

        public void UpdateView(Sprite towerIcon, int towerBuildPrice)
        {
            _towerIcon.sprite = towerIcon;
            _towerBuildPriceText.text = towerBuildPrice.ToString();
        }
    }
}
