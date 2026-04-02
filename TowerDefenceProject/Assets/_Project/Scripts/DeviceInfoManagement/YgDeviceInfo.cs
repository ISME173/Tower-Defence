using UnityEngine;
using YG;

namespace _Project.Scripts.DeviceInfoManagement
{
    public class YgDeviceInfo : IDeviceInfo
    {
        private IDeviceInfo.DeviceType _currentDeviceType;

        public IDeviceInfo.DeviceType CurrentDeviceType => _currentDeviceType;

        public YgDeviceInfo()
        {
            if (YG2.envir.isDesktop)
                _currentDeviceType = IDeviceInfo.DeviceType.Desktop;
            else if (YG2.envir.isMobile)
                _currentDeviceType = IDeviceInfo.DeviceType.Mobile;
            else
            {
                _currentDeviceType = IDeviceInfo.DeviceType.Unknown;
                Debug.LogError($"Invalid device type!");
            }
        }
    }
}
