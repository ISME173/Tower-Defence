using _Project.Scripts.Localization;
using _Project.Scripts.Saves;
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
        private ILocalizationInfo _localizationInfo;

        private readonly Subject<Unit> ReadOnlyOnTutorialFinished = new();

        public bool TutorialIsFinished => _saves != null && (_saves.HasKey(IsShowedTutorialSaveKey) && _saves.GetBool(IsShowedTutorialSaveKey));
        public Observable<Unit> OnTutorialFinished => ReadOnlyOnTutorialFinished;

        public TrainingController(TrainingSettings trainingSettings, TrainingView trainingView)
        {
            Settings = trainingSettings;
            TrainingView = trainingView;

            foreach (var stage in Settings.TrainingStages)
                stage.StageTriggerListener.Initialize();

            foreach (var entitySwitchDuringTraining in Settings.EntitysSwitchDuringTraining)
                entitySwitchDuringTraining.Initialize();
        }

        public void Initialize(ISaves saves, ILocalizationInfo localizationInfo)
        {
            _saves = saves;
            _localizationInfo = localizationInfo;

            if (TutorialIsFinished == false)
            {
                _currentTrainingStageIndex = 0;

                DisposablesByTrainingStageListeners.Add(Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener,
                    Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener.OnStageTriggerActivated.Subscribe(OnTriggerStageActivated));

                TrainingView.ShowInfoPanel();

                if (Settings.TrainingStages[_currentTrainingStageIndex].InfoText.TryGetTextByLang(_localizationInfo.CurrentLanguageType, out string text))
                    TrainingView.SetInfoText(text);

                foreach (var entitySwitchDuringTraining in Settings.EntitysSwitchDuringTraining)
                    entitySwitchDuringTraining.Disable();

                foreach (var objectForActivate in Settings.TrainingStages[_currentTrainingStageIndex].ObjectsForActivate)
                    objectForActivate.SetActive(true);

                foreach (var objectForDeactivate in Settings.TrainingStages[_currentTrainingStageIndex].ObjectsForDeactivate)
                    objectForDeactivate.SetActive(false);
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

            foreach (var objectForActivate in Settings.TrainingStages[_currentTrainingStageIndex].ObjectsForActivate)
                objectForActivate.SetActive(true);

            foreach (var objectForDeactivate in Settings.TrainingStages[_currentTrainingStageIndex].ObjectsForDeactivate)
                objectForDeactivate.SetActive(false);

            _currentTrainingStageIndex++;

            if (Settings.TrainingStages.Count <= _currentTrainingStageIndex)
            {
                TrainingView.HideInfoPanel();

                _saves.SetBool(IsShowedTutorialSaveKey, true);
                _saves.Save();

                foreach (var entitySwitchDuringTraining in Settings.EntitysSwitchDuringTraining)
                    entitySwitchDuringTraining.Enable();

                ReadOnlyOnTutorialFinished.OnNext(Unit.Default);

                return;
            }

            DisposablesByTrainingStageListeners.Add(Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener,
                Settings.TrainingStages[_currentTrainingStageIndex].StageTriggerListener.OnStageTriggerActivated.Subscribe(OnTriggerStageActivated));

            TrainingView.ShowInfoPanel();
            if (Settings.TrainingStages[_currentTrainingStageIndex].InfoText.TryGetTextByLang(_localizationInfo.CurrentLanguageType, out string text))
                TrainingView.SetInfoText(text);
        }


        [Serializable]
        public struct TrainingSettings
        {
            [SerializeField] private List<TrainingStage> _trainingStages;
            [SerializeField] private List<EntitySwitchDuringTraining> _entitysSwitchDuringTraining;

            public IReadOnlyList<TrainingStage> TrainingStages => _trainingStages;
            public IReadOnlyList<EntitySwitchDuringTraining> EntitysSwitchDuringTraining => _entitysSwitchDuringTraining;

            [Serializable]
            public struct TrainingStage
            {
                [SerializeField] private LocalizationVariants _infoText;
                [Space]
                [SerializeField] private TrainingStageTriggerListener _trainingStageTriggerListener;
                [Space]
                [SerializeField] private List<GameObject> _objectsForActivate;
                [SerializeField] private List<GameObject> _objectsForDeactivate;

                public LocalizationVariants InfoText => _infoText;
                public TrainingStageTriggerListener StageTriggerListener => _trainingStageTriggerListener;
                public IReadOnlyList<GameObject> ObjectsForActivate => _objectsForActivate;
                public IReadOnlyList<GameObject> ObjectsForDeactivate => _objectsForDeactivate;
            }
        }
    }
}
