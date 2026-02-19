using NaughtyAttributes;
using Reflex.Attributes;
using UnityEngine;

namespace _Project.Scripts.Saves
{
    public sealed class ClearAllSavesButton : MonoBehaviour
    {
        private ISaves _saves;

        [Inject]
        private void Construct(ISaves saves)
        {
            _saves = saves;
        }

        [Button("Clear All Saves")]
        [ContextMenu("Clear All Saves")]
        private void ClearAllSaves()
        {
            var saves = _saves;

            if (saves == null)
            {
                Debug.LogWarning($"[{nameof(ClearAllSavesButton)}] ISaves is not injected. Enter Play Mode!.");
                return;
            }

            saves.ClearAllSaves();
            Debug.Log($"[{nameof(ClearAllSavesButton)}] All saves cleared via {saves.GetType().Name}.");
        }
    }
}