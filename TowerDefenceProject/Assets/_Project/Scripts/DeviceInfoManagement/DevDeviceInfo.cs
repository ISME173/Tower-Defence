using UnityEngine;

namespace _Project.Scripts.DeviceInfoManagement
{
    public class DevDeviceInfo : IDeviceInfo
    {
        private IDeviceInfo.DeviceType _currentDeviceType;

        public IDeviceInfo.DeviceType CurrentDeviceType => _currentDeviceType;

        public DevDeviceInfo()
        {
            _currentDeviceType = IDeviceInfo.DeviceType.Mobile;
            return;

            switch (SystemInfo.deviceType)
            {
                case DeviceType.Handheld:
                    _currentDeviceType = IDeviceInfo.DeviceType.Mobile;
                    break;
                case DeviceType.Unknown:
                    _currentDeviceType = IDeviceInfo.DeviceType.Unknown;
                    break;
                case DeviceType.Desktop:
                    _currentDeviceType = IDeviceInfo.DeviceType.Desktop;
                    break;
                default:
                    throw new System.Exception($"Invalid device type: {SystemInfo.deviceType}");
            }
        }
    }
}
