using _Project.Scripts.EnemiesManagement;
using LitMotion;
using LitMotion.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Magic
{
    public class MagicTower : Tower
    {
        private readonly Dictionary<Projectile, MotionHandle> SpawnedProjectilesWithMotionHandles = new();

        [Header("Magic Tower References")]
        [SerializeField] private Transform _cristalsForAnimate;

        private MotionHandle _cristalsBobAnimateHandle;
        private MotionHandle _cristalsRotateAnimateHandle;
        private MagicTowerData _magicTowerData;

        public override void Initialize()
        {
            base.Initialize();

            float maxYPosition = _cristalsForAnimate.transform.localPosition.y + _magicTowerData.AnimateCristalYOffset;
            float minYPosition = _cristalsForAnimate.transform.localPosition.y - _magicTowerData.AnimateCristalYOffset;

            Vector3 startPosition = new Vector3(_cristalsForAnimate.localPosition.x, minYPosition, _cristalsForAnimate.localPosition.z);
            Vector3 endPosition = new Vector3(_cristalsForAnimate.localPosition.x, maxYPosition, _cristalsForAnimate.localPosition.z);

            _cristalsBobAnimateHandle = LMotion.Create(startPosition, endPosition, _magicTowerData.AnimateCristalsBobSpeed)
                .WithCancelOnError()
                .WithLoops(-1, LoopType.Yoyo)
                .WithEase(Ease.OutQuad)
                .BindToLocalPosition(_cristalsForAnimate);

            _cristalsRotateAnimateHandle = LMotion.Create(Vector3.zero, new Vector3(0, 360, 0), _magicTowerData.AnimateCristalsRotationSpeed)
                .WithCancelOnError()
                .WithLoops(-1, LoopType.Restart)
                .WithEase(Ease.Linear)
                .BindToEulerAngles(_cristalsForAnimate);
        }

        public override void Dispose()
        {
            base.Dispose();

            _cristalsBobAnimateHandle.TryCancel();
            _cristalsRotateAnimateHandle.TryCancel();
        }

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
                    Vector3 targetPosition = enemy.Center.position;

                    projectile.Transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, progress);
                    projectile.Transform.LookAt(targetPosition);
                });

            SpawnedProjectilesWithMotionHandles.Add(projectile, motionHandle.Value);
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

        protected override void ClearSpawnedProjectiles()
        {
            foreach (var key in SpawnedProjectilesWithMotionHandles.Keys)
            {
                ProjectilesPool.AddObject(key);
                SpawnedProjectilesWithMotionHandles[key].TryCancel();
            }

            SpawnedProjectilesWithMotionHandles.Clear();
        }
    }
}
