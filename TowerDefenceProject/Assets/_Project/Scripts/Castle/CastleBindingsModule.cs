using _Project.Scripts.DI;
using _Project.Scripts.EnemiesManagement.Spawn;
using _Project.Scripts.LevelsManagement;
using _Project.Scripts.Utilities;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.Castle
{
    public class CastleBindingsModule : BindingModule
    {
        [Header("References")]
        [SerializeField] private CastleHealthView _castleHealthView;

        [Header("Settings")]
        [SerializeField, Min(0)] private int _addHeartsCountAfterWatchAdv;
        [SerializeField] private LMotionShakeSerializableSettings _shakeSerializableSettings;

        private CastleEffectsManagement _castleEffectsManagement;
        private CastleHealthManagement _castleHealthManagement;

        private void OnDestroy()
        {
            _castleHealthManagement?.Dispose();
            _castleHealthView?.Dispose();
            _castleEffectsManagement?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _castleHealthManagement = new CastleHealthManagement(_castleHealthView, _addHeartsCountAfterWatchAdv);
            _castleEffectsManagement = new CastleEffectsManagement(_shakeSerializableSettings, _castleHealthManagement);

            containerBuilder.RegisterValue(_castleHealthManagement);
            containerBuilder.RegisterValue(_castleHealthView);
        }

        [Inject]
        private void Initialize(LevelsCreator levelsCreator, EnemiesSpawner enemiesSpawner, LevelCompletionManagement levelCompletionManagement)
        {
            _castleHealthManagement.Initialize(enemiesSpawner, levelsCreator, levelCompletionManagement);
            _castleEffectsManagement.Initialize(levelsCreator);
        }
    }
}
