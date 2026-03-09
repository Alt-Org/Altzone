using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;
using static SettingsCarrier;

// [CreateAssetMenu(fileName = "LanguageFlagMap")]
public class LanguageFlagMap : ScriptableObject
{
    [SerializeField] private List<LanguageFlag> _languageFlags;

    public Sprite GetFlag(LanguageType language)
    {
        Language convertedLanguage = ConvertLanguage(language);
        return _languageFlags.Find(langFlag => convertedLanguage == langFlag.language).flag;
    }

    public Sprite GetFlag(Language language)
    {
        return _languageFlags.Find(langFlag => language == langFlag.language).flag;
    }

    private Language ConvertLanguage(LanguageType language)
    {
        return language switch
        {
            LanguageType.English => Language.English,
            LanguageType.Finnish => Language.Finnish,
            LanguageType.Swedish => Language.Swedish,
            _ => Language.None,
        };
    }
}

[Serializable]
public class LanguageFlag
{
    public Language language;
    public Sprite flag;
}
