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
    private Language _tempLanguage;

    public void Initialize(Language firstSelected, bool includeNoneOption = false)
    {
        SelectedLanguage = firstSelected;
        _tempLanguage = SelectedLanguage;

        foreach (Transform child in _listParent)
        {
            Destroy(child.gameObject);
        }

        Language[] allowedLanguages = includeNoneOption
            ? new[]
            {
            Language.None,
            Language.Finnish,
            Language.English
            }
            : new[]
            {
            Language.Finnish,
            Language.English
            };

        foreach (Language language in allowedLanguages)
        {
            Sprite flag = _languageFlagMap.GetFlag(language);

            GameObject listItem = Instantiate(_languageListItemPrefab, _listParent);

            Toggle toggle = listItem.GetComponent<Toggle>();
            toggle.group = _toggleGroup;

            listItem.GetComponent<ClanLanguageListItem>().Initialize(
                language,
                flag,
                language == SelectedLanguage,
                (bool isOn) =>
                {
                    if (isOn)
                    {
                        _tempLanguage = language;
                    }
                });
        }
    }

    public Language SaveLanguage()
    {
        SelectedLanguage = _tempLanguage;
        return SelectedLanguage;
    }
}
