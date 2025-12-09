using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts;
using UnityEngine.UI;
using Altzone.Scripts.Window;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClanMainView : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _inClanPanel;
    [SerializeField] private GameObject _noClanPanel;
    [SerializeField] private GameObject _clanSettings;

    [SerializeField] private ClanLeaderboard _leaderboard;

    [Header("Text fields")]
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanPhrase;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private TextMeshProUGUI _clanWinsRanking;
    //[SerializeField] private TextMeshProUGUI _clanActivityRanking;
    [SerializeField] private TextMeshProUGUI _clanPassword;
    [SerializeField] private TextMeshProUGUI _clanGoal;
    [SerializeField] private TextMeshProUGUI _rule1Text;
    [SerializeField] private TextMeshProUGUI _rule2Text;
    [SerializeField] private TextMeshProUGUI _rule3Text;

    [Header("Other settings fields")]
    [SerializeField] GameObject _clanOpenObject;
    [SerializeField] GameObject _clanLockedObject;
    [SerializeField] LanguageFlagImage _flagImage;
    [SerializeField] GameObject _inClanButtons;
    [SerializeField] GameObject _notInClanButtons;
    [SerializeField] GameObject _editViewButtons;
    [SerializeField] ClanValuePanel _valuePanel;
    [SerializeField] ClanHeartColorSetter _clanHeart;

    [Header("Buttons")]
    [SerializeField] private Button _joinClanButton;
    [SerializeField] private Button _leaveClanButton;
    [SerializeField] private Button _linkButton;
    [SerializeField] private GameObject _editButton;

    [Header("pop ups")]
    [SerializeField] private ClanSearchPopup _clanPopup;
    [SerializeField] private GameObject _overlay;
    [SerializeField] private ClanConfirmPopup _confirmPopup;

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

    //prevents multiple join requests
    private bool _isJoining;

    private void OnEnable()
    {
        // Clear selection to prevent button highlight staying on screen
        EventSystem.current.SetSelectedGameObject(null);

        ResetViewState();

        ToggleClanPanel(false);
        OpenLink();

        ServerClan clan = DataCarrier.GetData<ServerClan>(DataCarrier.ClanListing, suppressWarning: true);

        if (clan != null && ServerManager.Instance.Clan != null &&
            clan._id == ServerManager.Instance.Clan._id)
        {
            clan = null;
        }

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
            Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
            {
                SetClanProfile(clanData);
            });

            _leaveClanButton.onClick.RemoveAllListeners();
            _leaveClanButton.onClick.AddListener(() =>
            {
                ShowLeaveClanPopUp();
            });
        }
        else
        {
            _clanHeart.SetOwnClanHeart = false;
            _inClanButtons.SetActive(false);
            _notInClanButtons.SetActive(true);

            if (_joinClanButton != null)
            {
                _joinClanButton.gameObject.SetActive(false);
            }
        }
    }

    private void ResetViewState()
    {
        if(_inClanPanel != null)
        {
            _inClanPanel.SetActive(true);
        }

        if (_clanSettings != null)
        {
            _clanSettings.SetActive(false);
        }

        if (_inClanButtons != null)
        {
            _inClanButtons.SetActive(true);
        }

        if (_editViewButtons != null)
        {
            _editViewButtons.SetActive(false);
        }

        if (_overlay != null)
        {
            _overlay.SetActive(false);
        }
    }

    private bool CanCurrentPlayerEditClan(ClanData clan)
    {
        var player = ServerManager.Instance.Player;
        if (player == null) return false;

        if (player.clan_id != clan.Id) return false;

        var roleId = player.clanRole_id;
        if (string.IsNullOrEmpty(roleId)) return false;

        var role = clan.ClanRoles?.Find(r => r._id == roleId);
        if (role?.rights == null) return false;

        return role.rights.edit_clan_data;
    }

    private void OpenLink()
    {
        string url = "https://altzone.fi/fi/clans/6740af56d977418ddbe08e29";
        _linkButton.onClick.RemoveAllListeners();
        _linkButton.onClick.AddListener(() =>
        {
            Application.OpenURL(url);
        });
    }

    public void UpdateProfileFromSettings(ClanData updatedClanData)
    {
        if (updatedClanData == null) return;    

        _clanHeart.SetOwnClanHeart = true;
        SetClanProfile(updatedClanData);
    }

    private void SetClanProfile(ClanData clan)
    {
        ToggleClanPanel(true);

        // Show correct buttons
        bool isInClan = ServerManager.Instance.Clan != null && clan.Id == ServerManager.Instance.Clan._id;
        _inClanButtons.SetActive(isInClan);
        _notInClanButtons.SetActive(!isInClan);
        if (_joinClanButton != null)
        {
            // Show join button only if not in clan
            _joinClanButton.gameObject.SetActive(!isInClan);
            _joinClanButton.interactable = clan.IsOpen && !isInClan;
        }

        if(_editButton != null)
        {
            _editButton.SetActive(CanCurrentPlayerEditClan(clan));
        }

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

        string rule1 = clan.Rules.Count > 0 ?
            _rule1Text.text = ClanDataTypeConverter.GetRulesText(clan.Rules[0]) : string.Empty;
        string rule2 = clan.Rules.Count > 1 ?
        _rule1Text.text = ClanDataTypeConverter.GetRulesText(clan.Rules[1]) : string.Empty;
        string rule3 = clan.Rules.Count > 2 ?
        _rule1Text.text = ClanDataTypeConverter.GetRulesText(clan.Rules[2]) : string.Empty;

        _rule1Text.text = rule1;
        _rule2Text.text = rule2;
        _rule3Text.text = rule3;

        // Temp values for testing
        //_clanActivityRanking.text = _clanWinsRanking.text = "-1";
        _clanPassword.text = "";
    }

    private void Reset()
    {
        ToggleClanPanel(false);
        _clanName.text = "Clan Name";
        _clanPhrase.text = "Clan Phrase";
        _clanMembers.text = _clanWinsRanking.text = "-1";
        _flagImage.SetFlag(Language.None);
        _inClanButtons.SetActive(false);
        _notInClanButtons.SetActive(true);

        if (_joinClanButton != null)
        {
            _joinClanButton.gameObject.SetActive(false);
        }

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
        if (_isJoining) return;
        _isJoining = true;

        StartCoroutine(ServerManager.Instance.JoinClan(clan, newClan =>
        {
            _isJoining = false;
            if (newClan == null) return;

            ServerManager.Instance.RaiseClanChangedEvent();
            SetClanProfile(new ClanData(newClan));

            if(_clanPopup)
            {
                _clanPopup.Hide();
            }
            ShowOverlay(false);           
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
            if (success)
            {
                Reset();             
            }
            onComplete?.Invoke(success);
        }));
    }

    public void ShowProfilePage()
    {
        if (_inClanPanel != null)
        {
            _inClanPanel.SetActive(true);
        }

        if (_clanSettings != null)
        {
            _clanSettings.SetActive(false);
        }
    }

    public void ShowSettingsPage()
    {
        if (_inClanPanel != null)
        {
            _inClanPanel.SetActive(false);
        }
        if (_clanSettings != null)
        {
            _clanSettings.SetActive(true);
        }
    }

    public void OnClickEditClanSettings()
    {
        ShowSettingsPage();

        if(_inClanButtons != null)
        {
           _inClanButtons.SetActive(false);
        }

        if(_editViewButtons != null)
        {
            _editViewButtons.SetActive(true);
        }
    }

    private void ShowOverlay (bool on)
    {
        _overlay.SetActive(on);
    }

    private string GetCurrentClanName()
    {
        var clanName = ServerManager.Instance.Clan;
        if (clanName != null)
        {
            return new ClanData(clanName).Name;
        }

        return "nykyisestä klaanista";
    }

    private void ShowLeaveClanPopUp()
    {
        var currentClanName = GetCurrentClanName();

        ShowOverlay(true);
        Debug.Log("[ClanMainView] ShowLeaveClanPopUp called, overlay enabled.");

        _confirmPopup.Show(
            bodyText: "Haluatko varmasti poistua klaanista " + currentClanName + "?",
            onConfirm: () =>
            {
                LeaveClan();
                ShowOverlay(false);
            },
            onCancel: () =>
            {
                ShowOverlay(false);
            },
            confirmText: "Poistu",
            cancelText: "Peruuta",
            style: "leave"
        );
    }


    private void ShowClanPopup(ServerClan clan)
    {
        ShowOverlay(true); 
        _clanPopup.Show(clan, onJoin: () =>
        {
            _clanPopup.Hide();
            if(ServerManager.Instance.Clan != null &&
            ServerManager.Instance.Clan._id != clan._id)
            {
                ShowLeaveAndJoinPopup(clan);
            }
            else
            {
                ShowJoinClanPopUp(clan);
            }
        });
    }

    public void CloseClanSearchPopup()
    {
        if(_clanPopup != null)
        {
            _clanPopup.Hide();
            
        }
        ShowOverlay(false);
    }

    private void ShowLeaveAndJoinPopup(ServerClan clan)
    {
        var currentClanName = GetCurrentClanName();
        var targetClanName = new ClanData(clan).Name;

        string warningText = "Olet jo jäsen klaanissa " + currentClanName + "." +
            " Haluatko varmasti poistua nykyisestä klaanista ja liittyä klaaniin " + targetClanName + "?";

        ShowOverlay(true);

        _confirmPopup.Show(
            bodyText: warningText,
            onConfirm: () =>
            {
                LeaveClan(success =>
                {
                    // first leave current clan, then join new clan
                    if (!success) { ShowOverlay(false); return; }
                    JoinClan(clan);              
                });                              
            },
            onCancel: () =>
            {
                ShowOverlay(false);
            },
            confirmText: "Liity",
            cancelText: "Peruuta",
            style: "leave"
            );
    }

    private void ShowJoinClanPopUp(ServerClan clan)
    {
        var targetClanName = new ClanData(clan).Name;

        ShowOverlay(true);

        _confirmPopup.Show(
            bodyText: "Haluatko liittyä klaaniin " + targetClanName + "?",

            onConfirm: () =>
            {
                if (ServerManager.Instance.Clan != null &&
                ServerManager.Instance.Clan._id != clan._id)
                {
                    ShowLeaveAndJoinPopup(clan);
                }
                else
                {
                    if(_clanPopup)
                    {
                        _clanPopup.Hide();
                    }
                    JoinClan(clan);
                }
            },
            onCancel: () =>
            {
                ShowOverlay(false);
            },
            confirmText: "Liity",
            cancelText: "Peruuta",
            style: "join"
            );
    }
}
