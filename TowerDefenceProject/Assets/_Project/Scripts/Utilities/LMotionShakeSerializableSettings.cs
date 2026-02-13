using System;
using UnityEngine;

namespace _Project.Scripts.Utilities
{
    [Serializable]
    public class LMotionShakeSerializableSettings
    {
        [SerializeField] private float _strenght;
        [SerializeField, Min(0)] private float _duration;

        public float Strenght => _strenght;
        public float Duration => _duration;
    }
}
