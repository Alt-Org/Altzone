using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClanSearchPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanDescription;
    [SerializeField] private TextLanguageSelectorCaller _clanMembers;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private TextMeshProUGUI _winsRankText;
    [SerializeField] private TextMeshProUGUI _activityRankText;
    [SerializeField] private Transform _labelsField;
    [SerializeField] private GameObject _labelImagePrefab;
    [SerializeField] private Button _joinClanButton;
    [SerializeField] private Button _openClanMainView;

    [SerializeField] private GameObject _clanOpenObject;
    [SerializeField] private GameObject _clanLockedObject;

    [SerializeField] private LanguageFlagImage _flagImage;
    [SerializeField] private Image _clanAgeImage;
    [SerializeField] private List<AgeIcon> _ageIcons = new List<AgeIcon>();

    [System.Serializable]
    private struct AgeIcon
    {
        public ClanAge age;
        public Sprite icon;
    }

    private Sprite GetAgeSprite(ClanAge age)
    {
        foreach (var ai in _ageIcons)
        {
            if (ai.age == age) return ai.icon;
        }
        return null;
    }

    private const int _maxClanMembers = 30;

    public void Show(ServerClan clan, UnityAction onJoin)
    {
        if (clan == null) return;
        ClanData clanData = new ClanData(clan);

        gameObject.SetActive(true);  

        _clanName.text = clanData.Name;
        _clanDescription.text = clanData.Phrase;
        _clanMembers.SetText(SettingsCarrier.Instance.Language, new string[1] { clan.playerCount + "/" + _maxClanMembers });
        _clanHeart.SetOtherClanColors(clanData);

        if (_clanOpenObject != null) _clanOpenObject.SetActive(clanData.IsOpen);
        if (_clanLockedObject != null) _clanLockedObject.SetActive(!clanData.IsOpen);

        if (_flagImage != null)
        {
            _flagImage.SetFlag(clanData.Language);
        }

        if (_clanAgeImage != null)
        {
            var ageSprite = GetAgeSprite(clanData.ClanAge);
            _clanAgeImage.sprite = ageSprite;
            _clanAgeImage.preserveAspect = true;
            _clanAgeImage.enabled = ageSprite != null;
        }

        if (_winsRankText) _winsRankText.text = "-";

        foreach (Transform child in _labelsField) Destroy(child.gameObject);
        foreach (ClanValues value in clanData.Values)
        {
            GameObject label = Instantiate(_labelImagePrefab, _labelsField);
            ValueImageHandle imageHandler = label.GetComponent<ValueImageHandle>();
            imageHandler.SetLabelInfo(value);
        }

        bool isMember =
            ServerManager.Instance.Clan != null &&
            clan._id == ServerManager.Instance.Clan._id;

        // Check if clan is full
        bool isFull = clan.playerCount >= _maxClanMembers;

        if (_joinClanButton != null)
        {
            _joinClanButton.gameObject.SetActive(!isMember);

            _joinClanButton.onClick.RemoveAllListeners();

            if (!isMember)
            {
                _joinClanButton.interactable = !isFull;

                var buttonText = _joinClanButton.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                {
                    if (isFull)
                    {
                        buttonText.text = "Klaani täynnä";
                    }
                    else
                    {
                        buttonText.text = "Liity";
                    }
                }

                if (!isFull)
                {
                    _joinClanButton.onClick.AddListener(() => { onJoin?.Invoke(); });
                }
                
            }
        }

        if(_openClanMainView != null)
        {
            _openClanMainView.gameObject.SetActive(isMember);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
