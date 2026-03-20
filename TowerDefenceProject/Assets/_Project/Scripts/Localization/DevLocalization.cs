using UnityEngine;

namespace _Project.Scripts.Localization
{
    public class DevLocalizationInfo : ILocalizationInfo
    {
        private LanguageType _currentLanguageType;

        public LanguageType CurrentLanguageType => _currentLanguageType;

        public DevLocalizationInfo()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                    _currentLanguageType = LanguageType.Russian;
                    break;
                case SystemLanguage.English:
                    _currentLanguageType = LanguageType.English;
                    break;
                case SystemLanguage.Turkish:
                    _currentLanguageType = LanguageType.Turkey;
                    break;
                case SystemLanguage.Spanish:
                    _currentLanguageType = LanguageType.Spanish;
                    break;
                case SystemLanguage.German:
                    _currentLanguageType = LanguageType.German;
                    break;
                case SystemLanguage.French:
                    _currentLanguageType = LanguageType.French;
                    break;
                default:
                    _currentLanguageType = LanguageType.English;
                    Debug.LogWarning($"The system language could not be determined. The default language has been set");
                    break;
            }
        }
    }
}
