using _Project.Scripts.CameraControll;
using _Project.Scripts.Saves;
using _Project.Scripts.Utilities;
using NaughtyAttributes;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Training
{
    public class TrainingController : IDisposable
    {
        private const string IsShowedTutorialSaveKey = "TutorialIsShowedKey";

        private readonly Dictionary<TrainingStageTriggerListener, IDisposable> DisposablesByTrainingStageListeners = new();
        private readonly TrainingSettings Settings;
        private readonly TrainingView TrainingView;

        private CameraMoving _cameraMoving;
        private int _currentTrainingStageIndex;
        private ISaves _saves;

        private readonly Subject<Unit> ReadOnlyOnTutorialFinished = new();

        public bool TutorialIsFinished => _saves != null && (_saves.HasKey(IsShowedTutorialSaveKey) && _saves.GetBool(IsShowedTutorialSaveKey));
        public Observable<Unit> OnTutorialFinished => ReadOnlyOnTutorialFinished;

        public TrainingController(TrainingSettings trainingSettings, TrainingView trainingView)
        {
            Settings = trainingSettings;
            TrainingView = trainingView;

            foreach (var stage in Settings.TrainingStages)
                stage.StageTriggerListener.Initialize();
        }

        public void Initialize(ISaves saves, CameraMoving cameraMoving)
        {
            _saves = saves;
            _cameraMoving = cameraMoving;

            if (_saves.HasKey(IsShowedTutorialSaveKey) == false || _saves.GetBool(IsShowedTutorialSaveKey) == false)
            {
                _currentTrainingStageIndex = 0;

                DisposablesByTrainingStageListeners.Add(Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener,
                    Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener.OnStageTriggerActivated.Subscribe(OnTriggerStageActivated));

                TrainingView.ShowInfoPanel();
                TrainingView.SetInfoText(Settings.TrainingStages[_currentTrainingStageIndex].InfoText);

                _cameraMoving.LockMoving();
            }
            else
            {
                ReadOnlyOnTutorialFinished.OnNext(Unit.Default);
            }
        }

        public void Dispose()
        {
            foreach (var key in DisposablesByTrainingStageListeners.Keys)
                DisposablesByTrainingStageListeners[key].Dispose();

            DisposablesByTrainingStageListeners.Clear();
        }

        private void OnTriggerStageActivated(Unit _)
        {
            DisposablesByTrainingStageListeners[Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener].Dispose();

            _currentTrainingStageIndex++;

            if (Settings.TrainingStages.Count <= _currentTrainingStageIndex)
            {
                TrainingView.HideInfoPanel();
                TrainingView.HideIndexFinger();

                _cameraMoving.UnlockMoving();
                _saves.SetBool(IsShowedTutorialSaveKey, true);

                ReadOnlyOnTutorialFinished.OnNext(Unit.Default);


                return;
            }

            DisposablesByTrainingStageListeners.Add(Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener,
                Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener.OnStageTriggerActivated.Subscribe(OnTriggerStageActivated));

            TrainingView.ShowInfoPanel();
            TrainingView.SetInfoText(Settings.TrainingStages[_currentTrainingStageIndex].InfoText);
        }


        [Serializable]
        public struct TrainingSettings
        {
            [SerializeField] private List<TrainingStage> _trainingStages;
            
            public enum MoveFingerType
            {
                MoveImmediately, MoveFromCurrentPosition
            }

            public IReadOnlyList<TrainingStage> TrainingStages => _trainingStages;

            [Serializable]
            public struct TrainingStage
            {
                [SerializeField, TextArea] private string _infoText;
                [Space]
                [SerializeField] private MoveFingerType _moveFingerType;
                [SerializeField] private Transform _moveFingerPoint;
                [Space]
                [SerializeField] private LMotionSerializableCustomSettings _clickAnimationSettings;
                [SerializeField] private LMotionSerializableCustomSettings _moveAnimationSettings;
                [Space]
                [SerializeField] private TrainingStageTriggerListener _trainingStageTriggerListener;

                public MoveFingerType MoveFingerType => _moveFingerType;
                public string InfoText => _infoText;
                public Transform MoveFingerPoint => _moveFingerPoint;
                public LMotionSerializableCustomSettings ClickAnimationSettings => _clickAnimationSettings;
                public LMotionSerializableCustomSettings MoveAnimationSettings => _moveAnimationSettings;
                public TrainingStageTriggerListener StageTriggerListener => _trainingStageTriggerListener;
            }
        }
    }
}
