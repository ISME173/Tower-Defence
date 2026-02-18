using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.Saves
{
    public class SavesModuleInstaller : MonoBehaviour, IInstaller
    {
        private ISaves _saves;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            _saves = new PlayerPrefsSaves();
            containerBuilder.RegisterValue(_saves);
        }
    }
}
