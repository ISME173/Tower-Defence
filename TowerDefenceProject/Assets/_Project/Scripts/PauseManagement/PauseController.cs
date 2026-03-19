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
        private const string SoundsVolumeSaveKey = "SoundsValueSaveKey";
        private const string MusicsVolumeSaveKey = "MusicsValueSaveKey";

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

            UpdateSettingsByCurrentSaves();
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

            float newMusicsVolume = _saves.GetFloat(MusicsVolumeSaveKey, 1) == 0 ? 1f : 0;
            _saves.SetFloat(MusicsVolumeSaveKey, newMusicsVolume);

            _audioService.SetCategoryVolume(AudioCategory.Music, newMusicsVolume);

            PauseView.SetActiveMusicView(newMusicsVolume == 1 ? true : false);
        }

        private void OnSfxActiveSwitchButtonClicked()
        {
            _audioService.PlayOneShot(_buttonClickAudioEvent);

            float newSoundsVolume = _saves.GetFloat(SoundsVolumeSaveKey, 1) == 0 ? 1f : 0;
            _saves.SetFloat(SoundsVolumeSaveKey, newSoundsVolume);

            _audioService.SetCategoryVolume(AudioCategory.Ui, newSoundsVolume);
            _audioService.SetCategoryVolume(AudioCategory.Sfx, newSoundsVolume);

            PauseView.SetActiveSoundsView(newSoundsVolume == 1 ? true : false);
        }

        private void UpdateSettingsByCurrentSaves()
        {
            float soundsVolume = _saves.GetFloat(SoundsVolumeSaveKey, 1);
            float musicsVolume = _saves.GetFloat(MusicsVolumeSaveKey, 1);

            _audioService.SetCategoryVolume(AudioCategory.Ui, soundsVolume);
            _audioService.SetCategoryVolume(AudioCategory.Sfx, soundsVolume);

            _audioService.SetCategoryVolume(AudioCategory.Music, musicsVolume);

            PauseView.SetActiveSoundsView(soundsVolume == 1 ? true : false);
            PauseView.SetActiveMusicView(musicsVolume == 1 ? true : false);
        }
    }
}
