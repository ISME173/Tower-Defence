using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Localization
{
    [Serializable]
    public class LocalizationVariants
    {
        [SerializeField] private List<TextByLanguage> _textsByLanguages = new List<TextByLanguage>();

        public bool TryGetTextByLang(LanguageType languageType, out string text)
        {
            text = string.Empty;

            if (_textsByLanguages == null)
                return false;

            foreach (var textByLanguage in _textsByLanguages)
            {
                if (textByLanguage.LanguageType == languageType)
                {
                    text = textByLanguage.Text;
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public struct TextByLanguage
    {
        [SerializeField] private LanguageType _languageType;
        [SerializeField, TextArea] private string _text;

        public LanguageType LanguageType => _languageType;
        public string Text => _text;
    }
}
