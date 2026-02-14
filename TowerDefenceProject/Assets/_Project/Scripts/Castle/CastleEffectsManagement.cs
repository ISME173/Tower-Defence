using _Project.Scripts.LevelsManagement;
using _Project.Scripts.Utilities;
using LitMotion;
using LitMotion.Extensions;
using R3;
using System;
using UnityEngine;

namespace _Project.Scripts.Castle
{

    public class CastleEffectsManagement : IDisposable
    {
        private readonly LMotionShakeSerializableSettings CastleTakeDamageMotionSettings;
        private readonly CastleHealthManagement CastleHealthManagement;

        private LevelsCreator _levelsCreator;
        private CompositeDisposable _compositeDisposable = new();
        private MotionHandle _takeDamageMotionHandle;

        public CastleEffectsManagement(LMotionShakeSerializableSettings castleTakeDamageMotionSettings, CastleHealthManagement castleHealthManagement)
        {
            CastleTakeDamageMotionSettings = castleTakeDamageMotionSettings;
            CastleHealthManagement = castleHealthManagement;
        }

        public void Initialize(LevelsCreator levelsCreator)
        {
            _levelsCreator = levelsCreator;

            _levelsCreator.ReadOnlyLevelCreated
                .Subscribe(_ => _takeDamageMotionHandle.TryCancel())
                .AddTo(_compositeDisposable);

            CastleHealthManagement.ReadOnlyCastleDamageTaken
                .Subscribe(OnCastleDamageTaken)
                .AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
            _takeDamageMotionHandle.TryCancel();
        }

        private void OnCastleDamageTaken(int currentHealth)
        {
            Debug.Log($"Castle damage taken. Current health: {currentHealth}");

            Transform castleView = _levelsCreator.CurrentLevelObject.CastleView.transform;
            Vector3 castlePosition = castleView.position;

            _takeDamageMotionHandle.TryCancel();

            _takeDamageMotionHandle = LMotion.Shake.Create(castlePosition.x, CastleTakeDamageMotionSettings.Strenght, CastleTakeDamageMotionSettings.Duration)
                .WithCancelOnError()
                .BindToPositionX(castleView);
        }
    }
}
