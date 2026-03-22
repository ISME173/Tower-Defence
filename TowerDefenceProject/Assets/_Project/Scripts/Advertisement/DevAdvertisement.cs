using System;
using UnityEngine;

namespace _Project.Scripts.Advertisement
{
    public class DevAdvertisement : IAdvertisement
    {
        public void ShowFullscreenAdv()
        {
            Debug.Log("<color=green>Fullscreen adv showed</color>");
        }

        public void ShowRewardedAdv(Action onSuccessfullyShowed, Action onError = null)
        {
            onSuccessfullyShowed?.Invoke();

            Debug.Log("<color=green>Rewarded adv showed</color>");
        }
    }
}
