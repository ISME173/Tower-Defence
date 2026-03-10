using UnityEngine;

namespace _Project.Scripts.Training
{
    [RequireComponent(typeof(IDisableDuringTraining))]
    public class EntitySwitchDuringTraining : MonoBehaviour
    {
        private IDisableDuringTraining _disableDuringTraining;

        public void Initialize()
        {
            _disableDuringTraining = GetComponent<IDisableDuringTraining>();
        }

        public void Disable()
        {
            if (_disableDuringTraining != null)
                _disableDuringTraining.Disable();
        }

        public void Enable()
        {
            if (_disableDuringTraining != null)
                _disableDuringTraining.Enable();
        }
    }
}
