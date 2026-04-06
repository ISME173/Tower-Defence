using System;
using UnityEngine;
using YG;

namespace _Project.Scripts.Advertisement
{
    public class YgAdvertisement : IAdvertisement
    {
        public void ShowFullscreenAdv()
        {
            bool changeTimeScale = Time.timeScale == 1;

            Time.timeScale = 0;

            YG2.onCloseInterAdv += OnCloseInterAdv;

            YG2.InterstitialAdvShow();

            void OnCloseInterAdv()
            {
                if (changeTimeScale)
                    Time.timeScale = 1;

                YG2.onCloseInterAdv -= OnCloseInterAdv;
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
