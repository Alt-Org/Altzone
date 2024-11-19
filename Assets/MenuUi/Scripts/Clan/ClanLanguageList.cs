using System;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;
using UnityEngine.UI;

public class ClanLanguageList : MonoBehaviour
{
    [SerializeField] private LanguageFlagMap _languageFlagMap;
    [SerializeField] private GameObject _languageListItemPrefab;
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Transform _listParent;
    public Language SelectedLanguage { get; private set; }

    public void Initialize(Language firstSelected)
    {
        SelectedLanguage = firstSelected;
        foreach (Transform child in _listParent) Destroy(child.gameObject);

        foreach (Language language in Enum.GetValues(typeof(Language)))
        {
            if (language == Language.None) continue;
            Sprite flag = _languageFlagMap.GetFlag(language);
            GameObject listItem = Instantiate(_languageListItemPrefab, _listParent);
            listItem.GetComponent<Toggle>().group = _toggleGroup;
            listItem.GetComponent<ClanLanguageListItem>().Initialize(language, flag, language == SelectedLanguage, (bool isOn) =>
            {
                SelectedLanguage = language;
            });
        }
    }
}
