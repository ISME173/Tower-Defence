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

        protected override void Update()
        {
            base.Update();

            if (TargetEnemy != null)
                RotateTurretToTarget(TargetEnemy.Transform);
        }

        public override void Initialize()
        {
            base.Initialize();

            _turretTowerData = TowerData as TurretTowerData;

            if (_turretTowerData == null)
            {
                Debug.LogError($"Invalid tower data type: {TowerData.GetType()}. Needed type: {GetTowerDataType()}");
            }
        }

        protected override void AttackEnemy(Enemy enemy)
        {
            enemy.TakeDamage(TowerData.AttackDamage);
        }

        protected override Type GetTowerDataType()
        {
            return typeof(TurretTowerData);
        }

        private void RotateTurretToTarget(Transform target)
        {
            _turretRotateTransform.LookAt(target);
        }
    }
}
