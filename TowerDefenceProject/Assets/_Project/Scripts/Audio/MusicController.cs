using _Project.Scripts.LevelsManagement;
using R3;
using Reflex.Attributes;
using System;
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
        private bool _isPlayingMusic = false;

        [Inject]
        private void Initialize(IAudioService audioService, LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement)
        {
            _audioService = audioService;
            _levelsCompletionManagement = levelCompletionManagement;
            _levelsCreator = levelsCreator;

            _levelsCreator.ReadOnlyLevelCreated
                .Subscribe(_ =>
                {
                    if (_audioService.GetCategoryVolume(AudioCategory.Music) > 0)
                    {
                        _audioService.Play(_backgroundMusicInGame);
                        _isPlayingMusic = true;
                    }
                })
                .AddTo(Disposables);

            _levelsCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(_ =>
                {
                    _audioService.Stop(_backgroundMusicInGame);
                    _isPlayingMusic = false;
                })
                .AddTo(Disposables);

            _levelsCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(_ =>
                {
                    _audioService.Stop(_backgroundMusicInGame);
                    _isPlayingMusic = false;
                })
                .AddTo(Disposables);

            _audioService.OnMusicsVolumeChanged
                .Subscribe(newValue =>
                {
                    if (newValue == 0)
                    {
                        _audioService.Stop(_backgroundMusicInGame);
                    }
                    else if (_isPlayingMusic == false)
                    {
                        _audioService.Play(_backgroundMusicInGame);
                        _isPlayingMusic = true;
                    }
                });
        }

        private void OnDestroy()
        {
            Disposables.Dispose();
        }
    }
}
