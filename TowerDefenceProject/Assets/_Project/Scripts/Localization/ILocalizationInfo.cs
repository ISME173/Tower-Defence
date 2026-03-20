using R3;
using UnityEngine;

namespace _Project.Scripts.Localization
{
    public enum LanguageType
    {
        Russian,
        English,
        Turkey,
        Spanish,
        German,
        French
    }

    public interface ILocalizationInfo
    {
        public LanguageType CurrentLanguageType { get; }
    }
}