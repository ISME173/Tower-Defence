using System;
using UnityEngine;

namespace _Project.Scripts.Utilities
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class Trigger : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetLayer;

        private Collider _collider;
        private Rigidbody _rigidbody;

        public event Action<Collider> OnTriggerEnterEvent;
        public event Action<Collider> OnTrggerExitEvent;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();

            _collider.isTrigger = true;

            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsInTargetLayer(other.gameObject))
            {
                return;
            }

            OnTriggerEnterEvent?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsInTargetLayer(other.gameObject))
            {
                return;
            }

            OnTrggerExitEvent?.Invoke(other);
        }

        private bool IsInTargetLayer(GameObject target)
        {
            return ((_targetLayer.value & (1 << target.layer)) != 0);
        }
    }
}
