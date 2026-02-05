using _Project.Scripts.Construction.TowerTypes.Cannon;
using _Project.Scripts.EnemiesManagement;
using LitMotion;
using System;
using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Magic
{
    public class MagicTower : Tower
    {
        private MagicTowerData _magicTowerData;

        protected override void AttackEnemy(Enemy enemy)
        {
            Projectile projectile = ProjectilesPool.GetObject();

            projectile.Transform.SetParent(ProjectilePointInWeapon);
            projectile.Transform.localPosition = Vector3.zero;
            projectile.Transform.localScale = Vector3.one;

            Vector3 startPosition = projectile.Transform.position;

            projectile.Transform.SetParent(null, true);

            float projectileFlyingTime = Vector3.Distance(startPosition, enemy.Center.position) / _magicTowerData.ProjectileSpeed;

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
            return typeof(MagicTowerData);
        }

        protected override void RotateWeaponToTarget(Transform targetTransform)
        {
            // No need to rotate
            return;
        }

        protected override void UpdateCurrentTowerData()
        {

        }

        protected override void UpdateWeaponProjectileView()
        {
            _magicTowerData = TowerData as MagicTowerData;
        }
    }
}
