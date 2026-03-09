using LitMotion;
using System;
using UnityEngine;

namespace _Project.Scripts.Utilities
{
    [Serializable]
    public struct LMotionSerializableCustomSettings
    {
        [SerializeField, Min(0)] private float _animationDuration;
        [SerializeField] private Ease _animationEase;
        [SerializeField, Min(-1)] private int _loopsCount;
        [SerializeField] private LoopType _loopType;

        public float AnimationDuration => _animationDuration;
        public LoopType LoopType => _loopType;
        public int LoopsCount => _loopsCount;
        public Ease AnimationEase => _animationEase;
    }
}
