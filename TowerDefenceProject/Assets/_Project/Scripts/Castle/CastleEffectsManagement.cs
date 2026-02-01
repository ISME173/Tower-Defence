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

        private CompositeDisposable _compositeDisposable = new();
        private LevelsCreator _levelsCreator;
        private MotionHandle _takeDamageMotionHandle;

        public CastleEffectsManagement(LMotionShakeSerializableSettings castleTakeDamageMotionSettings, CastleHealthManagement castleHealthManagement)
        {
            CastleTakeDamageMotionSettings = castleTakeDamageMotionSettings;
            CastleHealthManagement = castleHealthManagement;
        }

        public void Initialize(LevelsCreator levelsCreator)
        {
            _levelsCreator = levelsCreator;

            CastleHealthManagement.ReadOnlyCastleDamageTaken
                .Subscribe(OnCastleDamageTaken)
                .AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        private void OnCastleDamageTaken(int currentHealth)
        {
            Debug.Log($"Castle damage taken. Current health: {currentHealth}");

            Transform castleView = _levelsCreator.CurrentLevelObject.CastleView.transform;
            Vector3 castlePosition = castleView.localPosition;

            _takeDamageMotionHandle.TryCancel();
            MotionSequenceBuilder motionSequenceBuilder = LSequence.Create();

            motionSequenceBuilder
                .Append(LMotion.Create(castlePosition.z, castlePosition.z + CastleTakeDamageMotionSettings.Strenght, CastleTakeDamageMotionSettings.Duration)
                       .BindToLocalPositionZ(castleView))
                .Append(LMotion.Create(castlePosition.z, castlePosition.z - CastleTakeDamageMotionSettings.Strenght, CastleTakeDamageMotionSettings.Duration)
                       .BindToLocalPositionZ(castleView));

            _takeDamageMotionHandle = motionSequenceBuilder.Run();

        }
    }
}
