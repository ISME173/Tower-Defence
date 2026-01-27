using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes
{
    public class TurretTower : Tower
    {
        [Header("Turret tower references")]
        [SerializeField] private Transform _turretRotateTransform;

        protected override void Update()
        {
            base.Update();

            if (TargetEnemy != null)
                RotateTurretToTarget(TargetEnemy.Transform);
        }

        protected override void AttackEnemy(Enemy.Enemy enemy)
        {
            enemy.TakeDamage(AttackDamage);
        }

        private void RotateTurretToTarget(Transform target)
        {
            _turretRotateTransform.LookAt(target);
        }
    }
}
