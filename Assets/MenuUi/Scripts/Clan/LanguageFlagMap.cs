using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;

// [CreateAssetMenu(fileName = "LanguageFlagMap")]
public class LanguageFlagMap : ScriptableObject
{
    [SerializeField] private List<LanguageFlag> _languageFlags;

    public Sprite GetFlag(Language language)
    {
        return _languageFlags.Find(langFlag => language == langFlag.language).flag;
    }
}

[Serializable]
public class LanguageFlag
{
    public Language language;
    public Sprite flag;
}