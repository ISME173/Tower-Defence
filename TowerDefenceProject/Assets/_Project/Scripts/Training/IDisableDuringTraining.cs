using UnityEngine;

namespace _Project.Scripts.Training
{
    public interface IDisableDuringTraining
    {
        public void Disable();
        public void Enable();
    }
}
