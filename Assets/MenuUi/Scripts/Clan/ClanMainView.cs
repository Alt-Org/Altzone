using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts;
using UnityEngine.UI;
using MenuUi.Scripts.Window;
using System.Collections.Generic;

public class ClanMainView : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _inClanPanel;
    [SerializeField] private GameObject _noClanPanel;

    [SerializeField] private ClanLeaderboard _leaderboard;

    [Header("Text fields")]
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanPhrase;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private TextMeshProUGUI _clanWinsRanking;
    [SerializeField] private TextMeshProUGUI _clanActivityRanking;
    [SerializeField] private TextMeshProUGUI _clanPassword;
    [SerializeField] private TextMeshProUGUI _clanGoal;
    [SerializeField] private TextMeshProUGUI _clanAge;

    [Header("Other settings fields")]
    [SerializeField] GameObject _clanOpenObject;
    [SerializeField] GameObject _clanLockedObject;
    [SerializeField] LanguageFlagImage _flagImage;
    [SerializeField] GameObject _inClanButtons;
    [SerializeField] GameObject _notInClanButtons;
    [SerializeField] ClanValuePanel _valuePanel;
    [SerializeField] ClanHeartColorSetter _clanHeart;

    [Header("Buttons")]
    [SerializeField] private Button _joinClanButton;

    private void OnEnable()
    {
        ToggleClanPanel(false);

        ServerClan clan = DataCarrier.GetData<ServerClan>(DataCarrier.ClanListing, supressWarning: true);
        if (clan != null)
        {
            ClanData data = new ClanData(clan);
            _clanHeart.SetOwnClanHeart = false;
            _clanHeart.SetOtherClanColors(data);
            SetClanProfile(data);

            _joinClanButton.onClick.RemoveAllListeners();
            _joinClanButton.onClick.AddListener(() => { JoinClan(clan); });
        }
        else if (ServerManager.Instance.Clan != null)
        {
            _clanHeart.SetOwnClanHeart = true;
            Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) => SetClanProfile(clanData));
        }
    }

    private void SetClanProfile(ClanData clan)
    {
        ToggleClanPanel(true);

        // Show correct buttons
        bool isInClan = ServerManager.Instance.Clan != null && clan.Id == ServerManager.Instance.Clan._id;
        _inClanButtons.SetActive(isInClan);
        _notInClanButtons.SetActive(!isInClan);
        _joinClanButton.interactable = clan.IsOpen;

        // Show clan profile data
        _clanName.text = clan.Name;
        _clanMembers.text = "Jäsenmäärä: " + clan.Members.Count;
        _clanPhrase.text = clan.Phrase;
        _flagImage.SetFlag(clan.Language);
        _clanGoal.text = ClanDataTypeConverter.GetGoalText(clan.Goals);
        _clanAge.text = ClanDataTypeConverter.GetAgeText(clan.ClanAge);

        _valuePanel.SetValues(clan.Values);

        _clanOpenObject.SetActive(clan.IsOpen);
        _clanLockedObject.SetActive(!clan.IsOpen);

        // Temp values for testing
        _clanActivityRanking.text = _clanWinsRanking.text = "-1";
        _clanPassword.text = "";
    }

    private void Reset()
    {
        ToggleClanPanel(false);
        _clanName.text = "Clan Name";
        _clanPhrase.text = "Clan Phrase";
        _clanMembers.text = _clanActivityRanking.text = _clanWinsRanking.text = "-1";
        _clanPassword.text = _clanGoal.text = _clanAge.text = "";
        _flagImage.SetFlag(Language.None);
    }

    private void ToggleClanPanel(bool isInClan)
    {
        _inClanPanel.SetActive(isInClan);
        _noClanPanel.SetActive(!isInClan);
    }

    public void JoinClan(ServerClan clan)
    {
        StartCoroutine(ServerManager.Instance.JoinClan(clan, clan =>
        {
            if (clan == null) return;

            ServerManager.Instance.RaiseClanChangedEvent();
            SetClanProfile(new ClanData(clan));
        }));
    }

    public void LeaveClan()
    {
        StartCoroutine(ServerManager.Instance.LeaveClan(success =>
        {
            if (success) Reset();
        }));
    }
}
