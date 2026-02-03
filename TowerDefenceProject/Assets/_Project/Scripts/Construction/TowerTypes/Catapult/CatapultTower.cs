using _Project.Scripts.EnemiesManagement;
using System;
using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Catapult
{
    public class CatapultTower : Tower
    {
        [Space]
        [SerializeField] private Transform _weaponHorizontalRotationPivot;

        private CatapultTowerData _catapultTowerData;

        public override void Initialize()
        {
            base.Initialize();

            _catapultTowerData = TowerData as CatapultTowerData;
        }

        protected override void AttackEnemy(Enemy enemy)
        {
            enemy.TakeDamage(TowerData.AttackDamage);
        }

        protected override Type GetTowerDataType()
        {
            return typeof(CatapultTowerData);
        }

        protected override void RotateWeaponToTarget(Transform targetTransform)
        {
            if (targetTransform == null)
                return;

            if (_weaponHorizontalRotationPivot == null)
                return;

            Vector3 targetPosition = targetTransform.position;

            // 1) Горизонт: yaw (по Y).
            Vector3 horizontalToTarget = targetPosition - _weaponHorizontalRotationPivot.position;
            horizontalToTarget.y = 0;

            if (horizontalToTarget.sqrMagnitude > 0.0001f)
                _weaponHorizontalRotationPivot.rotation = Quaternion.LookRotation(horizontalToTarget, Vector3.up);
        }
    }
}
