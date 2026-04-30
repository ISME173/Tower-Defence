using Reflex.Attributes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizationText : MonoBehaviour
    {
        [SerializeField] private LocalizationVariants _localizationVariants;

        private TMP_Text _text;

        [Inject]
        private void Initialize(ILocalizationInfo localizationInfo)
        {
            _text = GetComponent<TMP_Text>();

            LanguageType currentLanguageType = localizationInfo.CurrentLanguageType;

            if (_localizationVariants.TryGetTextByLang(currentLanguageType, out string text))
            {
                _text.text = text;
            }
            else
            {
                Debug.LogWarning($"Not founded localization by language: {currentLanguageType} in GameObject '{gameObject.name}', with instance id: {gameObject.GetInstanceID()}");
            }
        }
    }
}
