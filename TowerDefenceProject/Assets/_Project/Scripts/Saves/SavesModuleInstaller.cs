using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.Saves
{
    public class SavesModuleInstaller : MonoBehaviour, IInstaller
    {
        private ISaves _saves;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            _saves = new YgSaves();
            containerBuilder.RegisterValue(_saves, new[] { typeof(ISaves) });
        }
    }
}
