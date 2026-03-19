using _Project.Scripts.Audio;
using _Project.Scripts.LevelsManagement;
using R3;
using System;

namespace _Project.Scripts.VictoryManagement
{
    public class VictoryController : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();
        private readonly VictoryView VictoryView;

        private LevelsCreator _levelsCreator;
        private LevelCompletionManagement _levelsCompletionManagement;

        private IAudioService _audioService;
        private AudioEvent _buttonClickAudioEvent;
        private AudioEvent _victoryAudioEvent;

        public Observable<Unit> ReadOnlyOnMenuButtonClicked => VictoryView.ReadOnlyOnMenuButtonClicked;

        public VictoryController(VictoryView victoryView)
        {
            VictoryView = victoryView;
        }

        public void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement levelsCompletionManagement,
            IAudioService audioService, AudioEvent buttonClickEvent, AudioEvent victoryAudioEvent)
        {
            _levelsCreator = levelsCreator;
            _levelsCompletionManagement = levelsCompletionManagement;

            _audioService = audioService;
            _victoryAudioEvent = victoryAudioEvent;
            _buttonClickAudioEvent = buttonClickEvent;

            _levelsCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(OnLevelCompleted)
                .AddTo(Disposables);

            VictoryView.Initialize();

            VictoryView.ReadOnlyOnContinueButtonClicked
                .Subscribe(OnContinueButtonDown)
                .AddTo(Disposables);

            VictoryView.ReadOnlyOnRestartButtonClicked
                .Subscribe(OnRestartButtonDown)
                .AddTo(Disposables);

            VictoryView.ReadOnlyOnMenuButtonClicked
                .Subscribe(_ =>
                {
                    _audioService.PlayOneShot(_buttonClickAudioEvent);
                    VictoryView.Hide();
                })
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            VictoryView?.Dispose();
            Disposables.Dispose();
        }

        private void OnLevelCompleted(Unit unit)
        {
            _audioService.PlayOneShot(_victoryAudioEvent);
            VictoryView.Show();
        }

        private void OnContinueButtonDown(Unit unit)
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);

            VictoryView.Hide();
            _levelsCreator.CreateNextLevel();
        }

        private void OnRestartButtonDown(Unit unit)
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);

            VictoryView.Hide();
            _levelsCreator.RebuildCurrentLevel();
        }
    }
}
