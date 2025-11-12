using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

    public void SetClanInfo(ServerClan clan, ClanListing clanListing)
    {
        if (clan != null)
        {
            ClanData clanData = new ClanData(clan);

            _clanName.text = clanData.Name;
            _clanDescription.text = clanData.Phrase;
            _clanMembers.SetText(SettingsCarrier.Instance.Language, new string[1] { clan.playerCount + "/25" });
            _clanHeart.SetOtherClanColors(clanData);
            _winsRankText.text = clanListing.WinsRank.text;
            _activityRankText.text = clanListing.ActivityRank.text;

            // Clan values
            foreach (Transform child in _labelsField) Destroy(child.gameObject);
            foreach (ClanValues value in clanData.Values)
            {
                GameObject label = Instantiate(_labelImagePrefab, _labelsField);
                ValueImageHandle imageHandler = label.GetComponent<ValueImageHandle>();
                imageHandler.SetLabelInfo(value);
            }

            _joinClanButton.onClick.AddListener(() => { clanListing.JoinButtonPressed(); });
        }
    }

    public void Show(ServerClan clan, UnityAction onJoin)
    {
        if (clan == null) return;
        ClanData clanData = new ClanData(clan);

        gameObject.SetActive(true);
        if (_joinClanButton) _joinClanButton.interactable = true;       

        _clanName.text = clanData.Name;
        _clanDescription.text = clanData.Phrase;
        _clanMembers.SetText(SettingsCarrier.Instance.Language, new string[1] { clan.playerCount + "/25" });
        _clanHeart.SetOtherClanColors(clanData);

        if(_winsRankText) _winsRankText.text = "-";

        foreach (Transform child in _labelsField) Destroy(child.gameObject);
        foreach (ClanValues value in clanData.Values)
        {
            GameObject label = Instantiate(_labelImagePrefab, _labelsField);
            ValueImageHandle imageHandler = label.GetComponent<ValueImageHandle>();
            imageHandler.SetLabelInfo(value);
        }

        _joinClanButton.onClick.RemoveAllListeners();
        _joinClanButton.onClick.AddListener(() => { onJoin?.Invoke(); });
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
