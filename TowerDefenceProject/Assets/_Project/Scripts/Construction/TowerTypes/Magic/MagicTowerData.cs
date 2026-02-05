using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Magic
{
    [CreateAssetMenu(fileName = "MagicTower", menuName = "Construction/MagicTowerData")]
    public class MagicTowerData : TowerData
    {
        [Header("Magic Tower Data")]
        [SerializeField, Min(0)] private float _animateCristalsRotationSpeed;
        [SerializeField, Min(0)] private float _animateCristalsBobSpeed;
        [SerializeField] private float _animateCristalsYOffset;

        public float AnimateCristalsRotationSpeed => _animateCristalsRotationSpeed;
        public float AnimateCristalsBobSpeed => _animateCristalsBobSpeed;
        public float AnimateCristalYOffset => _animateCristalsYOffset;
    }
}
