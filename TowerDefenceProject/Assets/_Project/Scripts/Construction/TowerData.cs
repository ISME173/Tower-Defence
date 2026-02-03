using UnityEngine;

namespace _Project.Scripts.Construction
{
    public abstract class TowerData : ScriptableObject
    {
        [Header("Base tower data")]
        [SerializeField, Min(0)] private int _attackDamage;
        [SerializeField, Min(0)] private float _delayBetweenAttacks;
        [SerializeField, Min(0)] private int _buildPrice;

        [Header("Base tower references")]
        [SerializeField] private Sprite _towerIconSprite;

        public int AttackDamage => _attackDamage;
        public float DelayBetweenAttacks => _delayBetweenAttacks;
        public int BuildPrice => _buildPrice;

        public Sprite TowerIconSprite => _towerIconSprite;
    }
}
