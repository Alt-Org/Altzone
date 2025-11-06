using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts;
using UnityEngine.UI;
using Altzone.Scripts.Window;
using System.Collections.Generic;
using UnityEngine.Events;

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
    [SerializeField] private Button _leaveClanButton;
    [SerializeField] private Button _linkButton;

    [Header("pop ups")]
    [SerializeField] private ClanLeavingPopUp _leaveClanPopUp;
    [SerializeField] private ClanJoiningPopUp _joinClanPopUp;
    [SerializeField] private ClanSearchPopup _clanPopup;
    [SerializeField] private GameObject _swipeBlockOverlay;

    [Header("Icons")]
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

    private void OnEnable()
    {
        ToggleClanPanel(false);
        OpenLink();

        ServerClan clan = DataCarrier.GetData<ServerClan>(DataCarrier.ClanListing, suppressWarning: true);
        if (clan != null)
        {
            ClanData data = new ClanData(clan);
            _clanHeart.SetOwnClanHeart = false;
            _clanHeart.SetOtherClanColors(data);
            SetClanProfile(data);

            _joinClanButton.onClick.RemoveAllListeners();
            _joinClanButton.onClick.AddListener(() => { ShowClanPopup(clan); });

        }
        else if (ServerManager.Instance.Clan != null)
        {
            _clanHeart.SetOwnClanHeart = true;
            Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) => SetClanProfile(clanData));

            _leaveClanButton.onClick.RemoveAllListeners();
            _leaveClanButton.onClick.AddListener(() => { ShowLeaveClanPopUp(); });
        }
        else
        {
            _clanHeart.SetOwnClanHeart = false;
            _inClanButtons.SetActive(false);
            _notInClanButtons.SetActive(true);
        }
    }

    private void OpenLink()
    {
        string url = "https://altzone.fi/fi/clans/6740af56d977418ddbe08e29";
        _linkButton.onClick.AddListener(() =>
        {
            Application.OpenURL(url);
        });
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
        _clanPhrase.text = string.IsNullOrWhiteSpace(clan.Phrase) ? "Klaanilla ei ole mottoa" : clan.Phrase;
        _flagImage.SetFlag(clan.Language);
        _clanGoal.text = ClanDataTypeConverter.GetGoalText(clan.Goals);

        var ageSprite = GetAgeSprite(clan.ClanAge);
        _clanAgeImage.sprite = ageSprite;
        _clanAgeImage.preserveAspect = true;
        _clanAgeImage.enabled = ageSprite != null;

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
        _flagImage.SetFlag(Language.None);
        _inClanButtons.SetActive(false);
        _notInClanButtons.SetActive(true);

        _clanPassword.text = _clanGoal.text = "";
        if (_clanAgeImage != null)
        {
            _clanAgeImage.sprite = null;
            _clanAgeImage.enabled = false;
        }
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

    public void LeaveClan(UnityAction<bool> onComplete)
    {
        StartCoroutine(ServerManager.Instance.LeaveClan(success =>
        {
            Debug.Log("[ClanMainView] LeaveClan callback, success=" + success);
            if (success) Reset();
            onComplete?.Invoke(success);
        }));
    }

    private void ShowOverlay (bool on)
    {
        _swipeBlockOverlay.SetActive(on);
    }

    private void ShowLeaveClanPopUp()
    {
        ShowOverlay(true);
        _leaveClanPopUp.Show(
            onConfirm: () =>
            {
                LeaveClan();
                ShowOverlay(false);
            },
            onCancel: () =>
            {
                ShowOverlay(false);
            });
    }

    private void ShowClanPopup(ServerClan clan)
    {
        ShowOverlay(true); 
        _clanPopup.Show(clan, onJoin: () =>
        {
            _clanPopup.Hide();
            ShowJoinClanPopUp(clan);
        });
    }


    private void ShowJoinClanPopUp(ServerClan clan)
    {
        ShowOverlay(true);
        _joinClanPopUp.Show(
            onConfirm: () =>
            {
                if (ServerManager.Instance.Clan != null && ServerManager.Instance.Clan._id != clan._id)
                {
                    _leaveClanPopUp.Show(
                        onConfirm: () =>
                        {
                            LeaveClan(success =>
                            {
                                if(!success) return;
                                JoinClan(clan);
                            });
                            
                            ShowOverlay(false);
                        },
                        onCancel: () =>
                        {
                            ShowOverlay(false);
                        });
                }
                else
                {
                    JoinClan(clan);
                    ShowOverlay(false);
                }

            },
            onCancel: () =>
            {
                ShowOverlay(false);
            });
    }
}
