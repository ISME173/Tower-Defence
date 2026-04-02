using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.DeviceInfoManagement
{
    public class DeviceInfoInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(new DevDeviceInfo(), new System.Type[] { typeof(IDeviceInfo) });
        }
    }
}
