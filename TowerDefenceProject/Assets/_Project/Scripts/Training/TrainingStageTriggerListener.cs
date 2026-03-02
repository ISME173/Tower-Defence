using R3;
using UnityEngine;

namespace _Project.Scripts.Training
{
    [RequireComponent(typeof(ITrainingStageTrigger))]
    public class TrainingStageTriggerListener : MonoBehaviour
    {
        private ITrainingStageTrigger _trainingStageTrigger;

        public Observable<Unit> OnStageTriggerActivated => _trainingStageTrigger.OnTrainingTriggerActivate;

        public void Initialize()
        {
            _trainingStageTrigger = GetComponent<ITrainingStageTrigger>();
        }
    }
}
