using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.Localization
{
    public class LocalizationInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(new YgLocalization(), new[] { typeof(ILocalizationInfo) });
        }
    }
}
