using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.Advertisement
{
    public class AdvertisementInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(new DevAdvertisement(), new[] { typeof(IAdvertisement) });
        }
    }
}
