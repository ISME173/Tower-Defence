using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes
{
    public class TurretTower : Tower
    {
        protected override void AttackEnemy(Enemy.Enemy enemy)
        {
            enemy.TakeDamage(AttackDamage);
        }
    }
}
