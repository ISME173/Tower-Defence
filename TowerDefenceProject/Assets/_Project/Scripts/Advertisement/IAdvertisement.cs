using System;

namespace _Project.Scripts.Advertisement
{
    public interface IAdvertisement
    {
        public void ShowFullscreenAdv();
        public void ShowRewardedAdv(Action onSuccessfullyShowed, Action onError = null);
    }
}
