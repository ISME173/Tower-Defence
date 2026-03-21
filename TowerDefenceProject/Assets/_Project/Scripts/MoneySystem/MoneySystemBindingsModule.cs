using _Project.Scripts.Advertisement;
using _Project.Scripts.Audio;
using _Project.Scripts.CameraControll;
using _Project.Scripts.DI;
using _Project.Scripts.EnemiesManagement.Spawn;
using _Project.Scripts.LevelsManagement;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.MoneySystem
{
    public class MoneySystemBindingsModule : BindingModule
    {
        [SerializeField] private MoneyView _moneyView;
        [SerializeField, Min(0)] private int _getMoneyCountAfterWatchAdv;

        [Header("SFX")]
        [SerializeField] private AudioEvent _buttonClickAudioEvent;

        private MoneyManagement _moneyManagement;

        private void OnDestroy()
        {
            _moneyManagement?.Dispose();
            _moneyView?.Dispose();
        }

        public override void Bind(ContainerBuilder containerBuilder)
        {
            _moneyManagement = new(_moneyView);

            containerBuilder.RegisterValue(_moneyManagement);
            containerBuilder.RegisterValue(_moneyView);
        }

        [Inject]
        private void Initialize(EnemiesSpawner enemiesSpawner, LevelsCreator levelsCreator, CameraMoving cameraMoving, LevelCompletionManagement levelCompletionManagement, IAdvertisement advertisement, IAudioService audioService)
        {
            _moneyManagement.Initialze(enemiesSpawner, levelsCreator, levelCompletionManagement, cameraMoving, advertisement, _getMoneyCountAfterWatchAdv,
                audioService, _buttonClickAudioEvent);
        }
    }
}
