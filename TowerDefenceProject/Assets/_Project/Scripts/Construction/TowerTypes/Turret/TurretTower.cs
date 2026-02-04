using _Project.Scripts.EnemiesManagement;
using System;
using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Turret
{
    public class TurretTower : Tower
    {
        [Header("Turret tower references")]
        [SerializeField] private Transform _turretRotateTransform;

        private TurretTowerData _turretTowerData;

        public override void Initialize()
        {
            base.Initialize();

            _turretTowerData = TowerData as TurretTowerData;
        }

        protected override void AttackEnemy(Enemy enemy)
        {
            enemy.TakeDamage(TowerData.AttackDamage);
        }

        protected override Type GetTowerDataType()
        {
            return typeof(TurretTowerData);
        }

        protected override void RotateWeaponToTarget(Transform targetTransform)
        {
            _turretRotateTransform.LookAt(targetTransform);
        }

        protected override void UpdateWeaponProjectileView()
        {
            
        }
    }
}
