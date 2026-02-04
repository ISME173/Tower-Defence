using _Project.Scripts.EnemiesManagement;
using LitMotion;
using System;
using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Ballista
{
    public class BallistaTower : Tower
    {
        [Space]
        [SerializeField] private Transform _weaponVerticalRotationPivot;
        [SerializeField] private Transform _weaponHorizontalRotationPivot;

        private BallistaTowerData _ballistaTowerData;

        public override void Initialize()
        {
            base.Initialize();

            _ballistaTowerData = TowerData as BallistaTowerData;
        }

        protected override void AttackEnemy(Enemy enemy)
        {
            Projectile projectile = ProjectilesPool.GetObject();

            projectile.Transform.SetParent(ProjectilePointInWeapon);
            projectile.Transform.localPosition = Vector3.zero;
            projectile.Transform.localScale = Vector3.one;

            Vector3 startPosition = projectile.Transform.position;

            projectile.Transform.SetParent(null, true);

            float projectileFlyingTime = Vector3.Distance(startPosition, enemy.Center.position) / _ballistaTowerData.ProjectileSpeed;

            MotionHandle? motionHandle = null;
            motionHandle = LMotion.Create(0f, 1f, projectileFlyingTime)
                .WithOnComplete(() =>
                {
                    enemy.TakeDamage(TowerData.AttackDamage);

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
            return typeof(BallistaTowerData);
        }

        protected override void RotateWeaponToTarget(Transform targetTransform)
        {
            if (targetTransform == null)
                return;

            if (_weaponHorizontalRotationPivot == null || _weaponVerticalRotationPivot == null)
                return;

            Vector3 targetPosition = targetTransform.position;

            Vector3 horizontalToTarget = targetPosition - _weaponHorizontalRotationPivot.position;
            horizontalToTarget.y = 0;

            if (horizontalToTarget.sqrMagnitude > 0.0001f)
                _weaponHorizontalRotationPivot.rotation = Quaternion.LookRotation(horizontalToTarget, Vector3.up);

            Vector3 localToTargetInHorizontal = _weaponHorizontalRotationPivot.InverseTransformPoint(targetPosition);

            if (localToTargetInHorizontal.sqrMagnitude <= 0.0001f)
                return;

            float pitchDegrees = -Mathf.Atan2(localToTargetInHorizontal.y, localToTargetInHorizontal.z) * Mathf.Rad2Deg;

            _weaponVerticalRotationPivot.localRotation = Quaternion.AngleAxis(pitchDegrees, Vector3.right);
        }

        protected override void UpdateWeaponProjectileView()
        {
            Projectile projectile = ProjectilesPool.GetObject();
            projectile.Transform.SetParent(ProjectilePointInWeapon);
            projectile.Transform.localPosition = Vector3.zero;
            projectile.Transform.localScale = Vector3.one;
            ProjectilesPool.AddObject(projectile);
            projectile.gameObject.SetActive(true);
            projectile.Transform.SetParent(ProjectilePointInWeapon);
        }
    }
}
