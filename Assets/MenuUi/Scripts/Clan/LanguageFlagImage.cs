using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageFlagImage : MonoBehaviour
{
    [SerializeField] private LanguageFlagMap _languageFlagMap;
    [SerializeField] Image _flagImage;
    [SerializeField] TextMeshProUGUI _languageText;

    public void SetFlag(Language language)
    {
        _flagImage.sprite = _languageFlagMap.GetFlag(language);
        if(_languageText != null) _languageText.text = ClanDataTypeConverter.GetLanguageText(language);
    }
}
