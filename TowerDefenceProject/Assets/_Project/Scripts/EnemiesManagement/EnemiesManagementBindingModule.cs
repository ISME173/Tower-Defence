using _Project.Scripts.Audio;
using _Project.Scripts.DI;
using _Project.Scripts.EnemiesManagement.Spawn;
using _Project.Scripts.LevelsManagement;
using _Project.Scripts.PauseManagement;
using _Project.Scripts.Training;
using NaughtyAttributes;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.EnemiesManagement
{

    public class EnemiesManagementBindingModule : BindingModule
    {
        [InfoBox("You can leave it blank and Transform will be used for this object", EInfoBoxType.Normal)]
        [SerializeField] private Transform _enemiesInPoolContainer;

        private EnemiesSpawner _enemiesSpawner;

        private void OnDestroy()
        {
            _enemiesSpawner?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _enemiesSpawner = new EnemiesSpawner();

            containerBuilder.RegisterValue(_enemiesSpawner);
        }

        [Inject]
        private void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement, PauseController pauseController, TrainingController trainingController, IAudioService audioService)
        {
            _enemiesSpawner.Initialize(levelsCreator, levelCompletionManagement, pauseController, _enemiesInPoolContainer ?? transform, trainingController, audioService);
        }
    }
}
