using _Project.Scripts.LevelsManagement;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace _Project.Scripts.Audio
{
    public class MusicController : MonoBehaviour
    {
        private readonly CompositeDisposable Disposables = new CompositeDisposable();

        [SerializeField] private AudioEvent _backgroundMusicInGame;

        private IAudioService _audioService;
        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelsCompletionManagement;

        [Inject]
        private void Initialize(IAudioService audioService, LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement)
        {
            _audioService = audioService;
            _levelsCompletionManagement = levelCompletionManagement;
            _levelsCreator = levelsCreator;

            _levelsCreator.ReadOnlyLevelCreated
                .Subscribe(_ => _audioService.Play(_backgroundMusicInGame))
                .AddTo(Disposables);

            _levelsCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(_ => _audioService.Stop(_backgroundMusicInGame))
                .AddTo(Disposables);

            _levelsCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(_ => _audioService.Stop(_backgroundMusicInGame))
                .AddTo(Disposables);
        }

        private void OnDestroy()
        {
            Disposables.Dispose();
        }
    }
}
