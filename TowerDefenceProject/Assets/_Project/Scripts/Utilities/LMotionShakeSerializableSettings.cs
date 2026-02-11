using System;
using UnityEngine;

namespace _Project.Scripts.Utilities
{
    [Serializable]
    public class LMotionShakeSerializableSettings
    {
        [SerializeField, Min(0)] private float _startValue;
        [SerializeField, Min(0)] private float _strenght;
        [SerializeField, Min(0)] private float _duration;

        public float StartValue => _startValue;
        public float Strenght => _strenght;
        public float Duration => _duration;
    }
}
