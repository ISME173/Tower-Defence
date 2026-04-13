using System;
using UnityEngine;
using YG;

namespace _Project.Scripts.Advertisement
{
    public class YgAdvertisement : IAdvertisement
    {
        public void ShowFullscreenAdv()
        {
            if (YG2.isTimerAdvCompleted == false)
                return;

            bool changeTimeScale = Time.timeScale == 1;

            Time.timeScale = 0;

            YG2.onCloseInterAdv += OnInterstitialAdvClosed;
            YG2.onErrorInterAdv += OnErrorInterstitialAdv;

            YG2.InterstitialAdvShow();

            void OnInterstitialAdvClosed()
            {
                if (changeTimeScale)
                    Time.timeScale = 1;

                YG2.onCloseInterAdv -= OnInterstitialAdvClosed;
                YG2.onErrorInterAdv -= OnErrorInterstitialAdv;

                Debug.Log($"Closed interstitial adv with change time scale value: {changeTimeScale}");
            }

            void OnErrorInterstitialAdv()
            {
                if (changeTimeScale)
                    Time.timeScale = 1;

                YG2.onCloseInterAdv -= OnInterstitialAdvClosed;
                YG2.onErrorInterAdv -= OnErrorInterstitialAdv;

                Debug.Log($"Error interstitial adv with change time scale value: {changeTimeScale}");
            }
        }

        public void ShowRewardedAdv(Action onSuccessfullyShowed, Action onError = null)
        {
            YG2.onErrorRewardedAdv += OnErrorRewardedAdv;

            YG2.RewardedAdvShow("defaultId", () =>
            {
                if (onSuccessfullyShowed != null)
                    onSuccessfullyShowed();

                //Debug.Log($"<color=green>Rewarded adv successfull showed!</color>");

                YG2.onErrorRewardedAdv -= OnErrorRewardedAdv;
            });

            void OnErrorRewardedAdv()
            {
                if (onError != null)
                    onError();

                //Debug.Log($"<color=red>Rewarded adv error!</color>");

                YG2.onErrorRewardedAdv -= OnErrorRewardedAdv;
            }
        }
    }
}
