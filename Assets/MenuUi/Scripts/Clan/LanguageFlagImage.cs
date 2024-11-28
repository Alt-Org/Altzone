using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;
using UnityEngine.UI;

public class LanguageFlagImage : MonoBehaviour
{
    [SerializeField] private LanguageFlagMap _languageFlagMap;
    [SerializeField] Image _flagImage;

    public void SetFlag(Language language) => _flagImage.sprite = _languageFlagMap.GetFlag(language);
}
