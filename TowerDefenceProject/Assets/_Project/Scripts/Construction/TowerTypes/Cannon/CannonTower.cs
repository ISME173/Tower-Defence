using _Project.Scripts.EnemiesManagement;
using LitMotion;
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

        protected override void AttackEnemy(Enemy enemy)
        {
            Projectile projectile = ProjectilesPool.GetObject();

            projectile.Transform.SetParent(ProjectilePointInWeapon);
            projectile.Transform.localPosition = Vector3.zero;
            projectile.Transform.localScale = Vector3.one;

            Vector3 startPosition = projectile.Transform.position;

            projectile.Transform.SetParent(null, true);

            float projectileFlyingTime = Vector3.Distance(startPosition, enemy.Center.position) / _cannonTowerData.ProjectileSpeed;

            MotionHandle? motionHandle = null;
            motionHandle = LMotion.Create(0f, 1f, projectileFlyingTime)
                .WithOnComplete(() =>
                {
                    enemy.TakeDamage(TowerData.AttackDamage, this);

                    ProjectilesPool.AddObject(projectile);
                })
                .Bind(progress =>
                {
                    Vector3 targetPosition = enemy.Center.position;

                    projectile.Transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, progress);
                    projectile.Transform.LookAt(targetPosition);
                });
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

        protected override void UpdateCurrentTowerData()
        {
            _cannonTowerData = TowerData as CannonTowerData;
        }

        protected override void UpdateWeaponProjectileView()
        {

        }
    }
}
