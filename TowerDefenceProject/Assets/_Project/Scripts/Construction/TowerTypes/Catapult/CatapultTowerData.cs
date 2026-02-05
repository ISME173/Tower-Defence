using UnityEngine;

namespace _Project.Scripts.Construction.TowerTypes.Catapult
{
    [CreateAssetMenu(fileName = "CatapultData", menuName = "Construction/CatapultData")]
    public class CatapultTowerData : TowerData
    {
        [Header("Catapult data")]
        [SerializeField, Min(0f)] private float _trajectoryHeight = 2.5f;
        [SerializeField] private Vector3 _weaponVerticalRotationInIdle = Vector3.zero;
        [SerializeField] private Vector3 _weaponVerticalRotationInAttack = Vector3.zero;
        [SerializeField, Min(0)] private float _rotateTime;

        public float RotateTime => _rotateTime;
        public float TrajectoryHeight => _trajectoryHeight;
        public Vector3 WeaponVerticalRotationInIdle => _weaponVerticalRotationInIdle;
        public Vector3 WeaponVerticalRotationInAttack => _weaponVerticalRotationInAttack;
    }
}
