using UnityEngine;

namespace _Project.Scripts.Construction
{
    public class Projectile : MonoBehaviour
    {
        private Transform _transform;

        public Transform Transform => _transform ?? transform;

        private void Awake()
        {
            _transform = transform;
        }
    }
}
