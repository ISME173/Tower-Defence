using _Project.Scripts.EnemiesManagement;
using System;
using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Magic
{
    public class MagicTower : Tower
    {
        private MagicTowerData _magicTowerData;

        protected override void AttackEnemy(Enemy enemy)
        {
            enemy.TakeDamage(TowerData.AttackDamage);
        }

        protected override Type GetTowerDataType()
        {
            return typeof(MagicTowerData);
        }

        protected override void RotateWeaponToTarget(Transform targetTransform)
        {
            // No need to rotate
            return;
        }

        protected override void UpdateCurrentTowerData()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateWeaponProjectileView()
        {
            _magicTowerData = TowerData as MagicTowerData;
        }
    }
}
