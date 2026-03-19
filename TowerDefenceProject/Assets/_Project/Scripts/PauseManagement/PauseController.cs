using _Project.Scripts.Audio;
using _Project.Scripts.CameraControll;
using _Project.Scripts.Saves;
using R3;
using System;
using UnityEngine;

namespace _Project.Scripts.PauseManagement
{
    public class PauseController : IDisposable
    {
        private readonly CompositeDisposable Disposables = new();
        private readonly PauseView PauseView;

        private CameraMoving _cameraMoving;
        private ISaves _saves;

        private IAudioService _audioService;
        private AudioEvent _buttonClickAudioEvent;

        public Observable<Unit> ReadOnlyOnOpenMenuButtonClicked => PauseView.ReadOnlyOnOpenMenuButtonClicked;

        public PauseController(PauseView pauseView)
        {
            PauseView = pauseView;
            
            PauseView.Initialize();

            PauseView.ReadOnlyOnShowPauseButtonClicked
                .Subscribe(_ => OnShowPauseButtonClicked())
                .AddTo(Disposables);

            PauseView.ReadOnlyOnHidePauseButtonClicked
                .Subscribe(_ => OnHidePauseButtonClicked())
                .AddTo(Disposables);

            PauseView.ReadOnlyOnMusicsActiveSwitchButtonClicked
                .Subscribe(_ => OnMusicsActiveSwitchButtonClicked())
                .AddTo (Disposables);

            PauseView.ReadOnlyOnSfxActiveSwitchButtonClicked
                .Subscribe(_ => OnSfxActiveSwitchButtonClicked())
                .AddTo(Disposables);

            ReadOnlyOnOpenMenuButtonClicked
                .Subscribe(_ =>
                {
                    OnHidePauseButtonClicked();
                    _cameraMoving.LockMoving();
                })
                .AddTo(Disposables);
        }

        public void Initialize(ISaves saves, CameraMoving cameraMoving, IAudioService audioService, AudioEvent buttonClickAudioEvent)
        {
            _saves = saves;
            _cameraMoving = cameraMoving;

            _audioService = audioService;
            _buttonClickAudioEvent = buttonClickAudioEvent;
        }

        public void Dispose()
        {
            PauseView?.Dispose();
            Disposables.Dispose();
        }

        private void OnShowPauseButtonClicked()
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);

            PauseView.Show();
            Time.timeScale = 0f;

            _cameraMoving.LockMoving();
        }

        private void OnHidePauseButtonClicked()
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);

            PauseView.Hide();
            Time.timeScale = 1f;

            _cameraMoving.UnlockMoving();
        }

        private void OnMusicsActiveSwitchButtonClicked()
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);
        }

        private void OnSfxActiveSwitchButtonClicked()
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);
        }
    }
}
