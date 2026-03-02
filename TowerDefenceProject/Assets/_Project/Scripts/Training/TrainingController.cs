using _Project.Scripts.Saves;
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

        private int _currentTrainingStageIndex;
        private ISaves _saves;

        private readonly Subject<Unit> ReadOnlyOnTutorialFinished = new();
        
        public Observable<Unit> OnTutorialFinished => ReadOnlyOnTutorialFinished;

        public TrainingController(TrainingSettings trainingSettings, TrainingView trainingView)
        {
            Settings = trainingSettings;
            TrainingView = trainingView;

            foreach (var stage in Settings.TrainingStages)
                stage.StageTriggerListener.Initialize();
        }

        public void Initialize(ISaves saves)
        {
            _saves = saves;

            if (_saves.HasKey(IsShowedTutorialSaveKey) == false || _saves.GetBool(IsShowedTutorialSaveKey) == false)
            {
                _currentTrainingStageIndex = 0;

                DisposablesByTrainingStageListeners.Add(Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener,
                    Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener.OnStageTriggerActivated.Subscribe(OnTriggerStageActivated));

                TrainingView.ShowInfoPanel();
                TrainingView.SetInfoText(Settings.TrainingStages[_currentTrainingStageIndex].InfoText);
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
            
            public IReadOnlyList<TrainingStage> TrainingStages => _trainingStages;

            [Serializable]
            public struct TrainingStage
            {
                [SerializeField, TextArea] private string _infoText;
                [SerializeField] private Vector3 _moveFingerPosition;
                [SerializeField] private bool _clickAnimationActive;
                [Space]
                [SerializeField] private TrainingStageTriggerListener _trainingStageTriggerListener;

                public string InfoText => _infoText;
                public Vector3 MoveFingerPosition => _moveFingerPosition;
                public bool ClickAnimationActive => _clickAnimationActive;
                public TrainingStageTriggerListener StageTriggerListener => _trainingStageTriggerListener;
            }
        }
    }
}
