using _Project.Scripts.DI;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.LevelsManagement
{

    public class LevelsManagementBindingsModule : BindingModule
    {
        [SerializeField] private Transform _createLevelPoint;
        [SerializeField] private LevelObject[] _levelObjectPrefabs;

        private LevelsCreator _levelsCreator;

        private void OnDestroy()
        {
            _levelsCreator?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _levelsCreator = new LevelsCreator(_levelObjectPrefabs, _createLevelPoint);

            containerBuilder.RegisterValue(_levelsCreator);
        }
    }
}
