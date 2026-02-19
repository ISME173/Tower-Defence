using _Project.Scripts.DI;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.CameraControll
{

    public class CameraBindingModule : BindingModule
    {
        [SerializeField] private CameraMoving _cameraMoving;

        public override void Bind(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(_cameraMoving);
        }
    }
}
