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
    [SerializeField] private TextMeshProUGUI _clanTrophies;
    [SerializeField] private TextMeshProUGUI _clanWinsRanking;
    [SerializeField] private TextMeshProUGUI _clanActivityRanking;
    [SerializeField] private TextMeshProUGUI _clanPassword;
    [SerializeField] private TextMeshProUGUI _clanGoal;
    [SerializeField] private TextMeshProUGUI _clanAge;

    [Header("Other settings fields")]
    [SerializeField] GameObject _clanOpenObject;
    [SerializeField] GameObject _clanLockedObject;
    [SerializeField] Image _flagImage;
    [SerializeField] GameObject _inClanButtons;
    [SerializeField] GameObject _notInClanButtons;
    [SerializeField] ClanValuePanel _valuePanel;

    [Header("Buttons")]
    [SerializeField] private Button _joinClanButton;

    [Header("Prefabs and scriptable objects")]
    [SerializeField] private LanguageFlagMap _languageFlagMap;

    private void OnEnable()
    {
        ToggleClanPanel(false);
        ServerClan clan = DataCarrier.GetData<ServerClan>(DataCarrier.ClanListing);
        if (clan != null)
        {
            SetClanProfile(new ClanData(clan));

            //ServerClan clan = DataCarrier.Instance.clanToView;
            //DataCarrier.Instance.clanToView = null;

            _joinClanButton.onClick.RemoveAllListeners();
            _joinClanButton.onClick.AddListener(() => { JoinClan(clan); });
        }
        else
        {
            Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) => SetClanProfile(clanData));
        }
    }

    private void SetClanProfile(ClanData clan)
    {
        ToggleClanPanel(true);

        bool isInClan = ServerManager.Instance.Clan != null && clan.Id == ServerManager.Instance.Clan._id;
        _inClanButtons.SetActive(isInClan);
        _notInClanButtons.SetActive(!isInClan);
        _joinClanButton.interactable = clan.IsOpen;

        _clanName.text = clan.Name;
        _clanMembers.text = "Jäsenmäärä: " + clan.Members.Count;
        _clanPhrase.text = clan.Phrase;
        _flagImage.sprite = _languageFlagMap.GetFlag(clan.Language);
        _clanGoal.text = ClanDataTypeConverter.GetGoalText(clan.Goals);
        _clanAge.text = ClanDataTypeConverter.GetAgeText(clan.ClanAge);

        _valuePanel.SetValues(clan.Values);

        _clanOpenObject.SetActive(clan.IsOpen);
        _clanLockedObject.SetActive(!clan.IsOpen);

        // Temp values for testing
        _clanTrophies.text = "-1";
        _clanActivityRanking.text = _clanWinsRanking.text = "-1";
        _clanPassword.text = "";
    }

    private void Reset()
    {
        ToggleClanPanel(false);
        _clanName.text = "Clan Name";
        _clanPhrase.text = "Clan Phrase";
        _clanMembers.text = _clanTrophies.text = _clanActivityRanking.text = _clanWinsRanking.text = "-1";
        _clanPassword.text = _clanGoal.text = _clanAge.text = "";
        _flagImage.sprite = _languageFlagMap.GetFlag(Language.None);
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
