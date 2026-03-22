using System;
using YG;

namespace _Project.Scripts.Advertisement
{
    public class YgAdvertisement : IAdvertisement
    {
        public void ShowFullscreenAdv()
        {
            YG2.InterstitialAdvShow();
        }

        public void ShowRewardedAdv(Action onSuccessfullyShowed, Action onError = null)
        {
            YG2.onErrorRewardedAdv += OnErrorRewardedAdv;

            YG2.RewardedAdvShow("defaultId", () =>
            {
                onSuccessfullyShowed?.Invoke();

                YG2.onErrorRewardedAdv -= OnErrorRewardedAdv;
            });

            void OnErrorRewardedAdv()
            {
                onError?.Invoke();

                YG2.onErrorRewardedAdv -= OnErrorRewardedAdv;
            }
        }
    }
}
