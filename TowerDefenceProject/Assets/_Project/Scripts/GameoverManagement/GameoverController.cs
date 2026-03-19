using _Project.Scripts.Audio;
using _Project.Scripts.LevelsManagement;
using R3;
using System;

namespace _Project.Scripts.GameoverMagamenet
{
    public class GameoverController : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();
        private readonly GameoverView GameoverView;

        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelCompletionManagement;

        private IAudioService _audioService;
        private AudioEvent _gameoverAudioEvent;
        private AudioEvent _buttonClickAudioEvent;

        public Observable<Unit> ReadOnlyOnMenuButtonClicked => GameoverView.ReadOnlyOnMenuButtonClicked;

        public GameoverController(GameoverView gameoverView)
        {
            GameoverView = gameoverView;
        }

        public void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement, IAudioService audioService,
            AudioEvent buttonClickAudioEvent, AudioEvent gameoverAudioEvent)
        {
            _levelsCreator = levelsCreator;
            _levelCompletionManagement = levelCompletionManagement;

            _audioService = audioService;
            _buttonClickAudioEvent = buttonClickAudioEvent;
            _gameoverAudioEvent = gameoverAudioEvent;

            GameoverView.Initialize();

            _levelCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(OnLevelFailed)
                .AddTo(Disposables);

            GameoverView.ReadOnlyOnRestartButtonClicked
                .Subscribe(OnRestartButtonClicked)
                .AddTo(Disposables);

            GameoverView.ReadOnlyOnMenuButtonClicked
                .Subscribe(_ =>
                {
                    _audioService.PlayOneShot(_buttonClickAudioEvent);
                    GameoverView.Hide();
                })
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            GameoverView?.Dispose();
            Disposables.Dispose();
        }

        private void OnRestartButtonClicked(Unit unit)
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);
            GameoverView.Hide();

            _levelsCreator.RebuildCurrentLevel();
        }

        private void OnLevelFailed(Unit unit)
        {
            _audioService.PlayOneShot(_gameoverAudioEvent);
            GameoverView.Show();
        }
    }
}
