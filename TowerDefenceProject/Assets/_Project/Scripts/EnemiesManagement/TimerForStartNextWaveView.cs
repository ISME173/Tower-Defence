using _Project.Scripts.EnemiesManagement.Spawn;
using R3;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.EnemiesManagement
{

    public class TimerForStartNextWaveView : MonoBehaviour
    {
        private readonly CompositeDisposable Disposables = new();
        [Inject] private readonly EnemiesSpawner EnemiesSpawner;

        [SerializeField] private RectTransform _viewForHideAndShow;
        [SerializeField] private TextMeshProUGUI _timerText;

        private void OnEnable()
        {
            EnemiesSpawner.ReadOnlySecondsRemainingUntilStartChanged
                .Subscribe(value =>
                {
                    _timerText.text = value.ToString();

                    if (value == 0)
                        _viewForHideAndShow.gameObject.SetActive(false);
                    else if (_viewForHideAndShow.gameObject.activeSelf == false)
                        _viewForHideAndShow.gameObject.SetActive(true);
                })
                .AddTo(Disposables);
        }

        private void OnDisable()
        {
            Disposables.Dispose();
        }
    }
}
