using UnityEngine;

namespace _Project.Scripts.DeviceInfoManagement
{
    public interface IDeviceInfo
    {
        public enum DeviceType
        {
            Unknown,
            Mobile,
            Desktop
        }

        public DeviceType CurrentDeviceType { get; }
    }
}
