using UnityEngine;
using YG;

namespace _Project.Scripts.Localization
{
    public class YgLocalization : ILocalizationInfo
    {
        private LanguageType _currentLanguageType;

        public LanguageType CurrentLanguageType => _currentLanguageType;

        public YgLocalization()
        {
            switch (YG2.lang)
            {
                case "ru":
                    _currentLanguageType = LanguageType.Russian;
                    break;
                case "en":
                    _currentLanguageType = LanguageType.English;
                    break;
                case "tr":
                    _currentLanguageType = LanguageType.Turkish;
                    break;
                case "es":
                    _currentLanguageType = LanguageType.Spanish;
                    break;
                case "fr":
                    _currentLanguageType = LanguageType.French;
                    break;
                case "de":
                    _currentLanguageType = LanguageType.German;
                    break;
                default:
                    _currentLanguageType = LanguageType.English;
                    break;
            }
        }
    }
}
