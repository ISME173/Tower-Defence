using UnityEngine;

namespace _Project.Scripts.Construction
{
    public abstract class TowerData : ScriptableObject
    {
        [Header("Base tower data")]
        [SerializeField, Min(0)] private int _attackDamage;
        [SerializeField, Min(0)] private float _delayBetweenAttacks;
        [SerializeField, Min(0)] private float _rotateWeaponToEnemySpeed;
        [SerializeField, Min(0)] private int _buildPrice;

        public int AttackDamage => _attackDamage;
        public float DelayBetweenAttacks => _delayBetweenAttacks;
        public float RotateWeaponTowerSpeed => _rotateWeaponToEnemySpeed;
        public int BuildPrice => _buildPrice;
    }
}
