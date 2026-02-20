using _Project.Scripts.EnemiesManagement;
using LitMotion;
using LitMotion.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Catapult
{
    public class CatapultTower : Tower
    {
        private readonly Dictionary<Projectile, MotionHandle> SpawnedProjectilesWithMotionHandles = new();

        [Space]
        [SerializeField] private Transform _weaponHorizontalRotationPivot;
        [SerializeField] private Transform _weaponVerticalRotationPivot;

        private CatapultTowerData _catapultTowerData;

        protected override void AttackEnemy(Enemy enemy)
        {
            if (enemy == null || enemy.Alive == false)
                return;

            Projectile projectile = ProjectilesPool.GetObject();

            projectile.Transform.SetParent(ProjectilePointInWeapon);
            projectile.Transform.localPosition = Vector3.zero;
            projectile.Transform.localScale = Vector3.one;

            Vector3 startPosition = projectile.Transform.position;

            //projectile.Transform.SetParent(null, true);

            float distance = Vector3.Distance(startPosition, enemy.Center.position);
            float projectileFlyingTime = distance / Mathf.Max(0.01f, _catapultTowerData.ProjectileSpeed);

            MotionHandle? motionHandle = null;

            MotionSequenceBuilder motionSequenceBuilder = LSequence.Create();

            motionHandle = motionSequenceBuilder
                .Append(LMotion.Create(_catapultTowerData.WeaponVerticalRotationInIdle, _catapultTowerData.WeaponVerticalRotationInAttack, _catapultTowerData.RotateTime)
                .WithCancelOnError()
                .WithOnComplete(() =>
                {
                    projectile.Transform.SetParent(null, true);
                })
                .BindToLocalEulerAngles(_weaponVerticalRotationPivot))
                .Append(LMotion.Create(0f, 1f, projectileFlyingTime)
                .WithCancelOnError()
                .WithOnComplete(() =>
                {
                    if (enemy != null && enemy.Alive)
                        enemy.TakeDamage(TowerData.AttackDamage, this);

                    ProjectilesPool.AddObject(projectile);

                    if (SpawnedProjectilesWithMotionHandles.ContainsKey(projectile))
                    {
                        SpawnedProjectilesWithMotionHandles[projectile].TryCancel();
                        SpawnedProjectilesWithMotionHandles.Remove(projectile);
                    }
                })
                .Bind(progress =>
                {
                    if (enemy == null || enemy.Center == null)
                        return;

                    Vector3 targetPosition = enemy.Center.position;

                    Vector3 basePosition = Vector3.LerpUnclamped(startPosition, targetPosition, progress);

                    float arc = 4f * _catapultTowerData.TrajectoryHeight * progress * (1f - progress);
                    Vector3 position = basePosition + Vector3.up * arc;

                    projectile.Transform.position = position;

                    Vector3 nextBasePosition = Vector3.LerpUnclamped(startPosition, targetPosition, Mathf.Min(1f, progress + 0.01f));
                    float nextArc = 4f * _catapultTowerData.TrajectoryHeight * Mathf.Min(1f, progress + 0.01f) * (1f - Mathf.Min(1f, progress + 0.01f));
                    Vector3 nextPosition = nextBasePosition + Vector3.up * nextArc;

                    Vector3 forward = nextPosition - position;
                    if (forward.sqrMagnitude > 0.0001f)
                        projectile.Transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                }))
                .Append(LMotion.Create(_catapultTowerData.WeaponVerticalRotationInAttack, _catapultTowerData.WeaponVerticalRotationInIdle, _catapultTowerData.RotateTime)
                .WithCancelOnError()
                .BindToLocalEulerAngles(_weaponVerticalRotationPivot))
                .Run();

            SpawnedProjectilesWithMotionHandles.Add(projectile, motionHandle.Value);
        }

        protected override void ClearSpawnedProjectiles()
        {
            foreach (var key in SpawnedProjectilesWithMotionHandles.Keys)
            {
                ProjectilesPool.AddObject(key);
                SpawnedProjectilesWithMotionHandles[key].TryCancel();
            }

            SpawnedProjectilesWithMotionHandles.Clear();
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

        protected override void UpdateCurrentTowerData()
        {
            _catapultTowerData = TowerData as CatapultTowerData;
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
