using _Project.Scripts.DI;
using _Project.Scripts.EnemiesManagement.Spawn;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.EnemiesManagement
{

    public class EnemiesManagementBindingModule : BindingModule
    {
        [SerializeField] private EnemiesSpawner _enemiesSpawner;

        public override void Bind(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(_enemiesSpawner);
        }
    }
}
