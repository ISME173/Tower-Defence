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
        [SerializeField] private List<TextByLanguage> _textsByLanguages = new List<TextByLanguage>();

        private TMP_Text _text;

        [Inject]
        private void Initialize(ILocalizationInfo localizationInfo)
        {
            ValidateTextsByLanguages();

            _text = GetComponent<TMP_Text>();

            LanguageType currentLanguageType = localizationInfo.CurrentLanguageType;

            foreach (var textByLanguage in _textsByLanguages)
            {
                if (textByLanguage.LanguageType == currentLanguageType)
                {
                    _text.text = textByLanguage.Text;
                    break;
                }
            }
        }

        private void ValidateTextsByLanguages()
        {
            if (_textsByLanguages == null || _textsByLanguages.Count == 0)
                return;

            HashSet<LanguageType> uniqueLanguageTypes = new HashSet<LanguageType>();

            for (int i = 0; i < _textsByLanguages.Count; i++)
            {
                TextByLanguage textByLanguage = _textsByLanguages[i];

                if (uniqueLanguageTypes.Add(textByLanguage.LanguageType) == false)
                {
                    Debug.LogWarning(
                        $"{nameof(LocalizationText)} on object {name}: duplicate language {textByLanguage.LanguageType} was removed from {nameof(_textsByLanguages)}.",
                        this);

                    _textsByLanguages.RemoveAt(i);
                    i--;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(textByLanguage.Text))
                {
                    Debug.LogWarning(
                        $"{nameof(LocalizationText)} on object {name}: translation text for language {textByLanguage.LanguageType} is empty.",
                        this);
                }
            }
        }

        [Serializable]
        private struct TextByLanguage
        {
            [SerializeField] private LanguageType _languageType;
            [SerializeField, TextArea] private string _text;

            public LanguageType LanguageType => _languageType;
            public string Text => _text;
        }
    }
}
