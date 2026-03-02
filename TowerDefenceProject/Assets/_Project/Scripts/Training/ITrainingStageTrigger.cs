using R3;

namespace _Project.Scripts.Training
{
    public interface ITrainingStageTrigger
    {
        public Observable<Unit> OnTrainingTriggerActivate { get; }
    }
}
