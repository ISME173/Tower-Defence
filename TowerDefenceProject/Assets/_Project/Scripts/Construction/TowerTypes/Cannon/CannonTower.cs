using _Project.Scripts.EnemiesManagement;
using System;
using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Cannon
{
    public class CannonTower : Tower
    {
        [Space]
        [SerializeField] private Transform _weaponVerticalRotationPivot;
        [SerializeField] private Transform _weaponHorizontalRotationPivot;

        private CannonTowerData _cannonTowerData;

        public override void Initialize()
        {
            base.Initialize();

            _cannonTowerData = TowerData as CannonTowerData;
        }

        protected override void AttackEnemy(Enemy enemy)
        {
            enemy.TakeDamage(_cannonTowerData.AttackDamage);
        }

        protected override Type GetTowerDataType()
        {
            return typeof(CannonTowerData);
        }

        protected override void RotateWeaponToTarget(Transform targetTransform)
        {
            if (targetTransform == null)
                return;

            if (_weaponHorizontalRotationPivot == null || _weaponVerticalRotationPivot == null)
                return;

            Vector3 targetPosition = targetTransform.position;

            // 1) Горизонт: yaw (по Y).
            Vector3 horizontalToTarget = targetPosition - _weaponHorizontalRotationPivot.position;
            horizontalToTarget.y = 0;

            if (horizontalToTarget.sqrMagnitude > 0.0001f)
                _weaponHorizontalRotationPivot.rotation = Quaternion.LookRotation(horizontalToTarget, Vector3.up);

            // 2) Вертикаль: pitch (по X) считаем в координатах HORIZONTAL pivot'а (родителя),
            // чтобы yaw родителя не ломал расчёт.
            Vector3 localToTargetInHorizontal = _weaponHorizontalRotationPivot.InverseTransformPoint(targetPosition);

            if (localToTargetInHorizontal.sqrMagnitude <= 0.0001f)
                return;

            float pitchDegrees = -Mathf.Atan2(localToTargetInHorizontal.y, localToTargetInHorizontal.z) * Mathf.Rad2Deg;

            _weaponVerticalRotationPivot.localRotation = Quaternion.AngleAxis(pitchDegrees, Vector3.right);
        }
    }
}
