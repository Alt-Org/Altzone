using UnityEngine;
using System.Collections;
using TMPro;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts;
using UnityEngine.UI;
using Altzone.Scripts.Window;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MenuUi.Scripts.TabLine;
using MenuUi.Scripts.SwipeNavigation;

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
    [SerializeField] GameObject _clanMemberPageButtons;
    [SerializeField] GameObject _editViewButtons;
    [SerializeField] ClanValuePanel _valuePanel;
    [SerializeField] ClanHeartColorSetter _clanHeart;
    [SerializeField] private ScrollRect _swipeScrollRect;
    [SerializeField] private TabLine _tabLine;
    [SerializeField] private GameObject _clanSwipeRoot;
    [SerializeField] private GameObject _tabButtonsRoot;
    [SerializeField] private ClanMembersPageController _membersPageController;

    [Header("Buttons")]
    [SerializeField] private Button _joinClanButton;
    [SerializeField] private Button _leaveClanButton;
    [SerializeField] private Button _linkButton;
    [SerializeField] private GameObject _editButton;
    [SerializeField] private GameObject _viewClansButton;

    private bool _isInClanCached;
    private bool _canEditCached;

    private bool _isProfilePageVisible = true;

    [Header("pop ups")]
    [SerializeField] private ClanSearchPopup _clanPopup;
    [SerializeField] private GameObject _overlay;
    [SerializeField] private ClanConfirmPopup _confirmPopup;
    [SerializeField] private ClanMemberPopupController _memberDetailsPopup;

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

    private enum ClanPage
    {
        Profile,
        Members,
        Settings
    }

    private ClanPage _currentPage = ClanPage.Profile;

    //prevents multiple join requests
    private bool _isJoining;

    private void OnEnable()
    {
        // Clear selection to prevent button highlight staying on screen
        EventSystem.current.SetSelectedGameObject(null);

        if (_tabLine != null && _tabLine.Swipe != null)
            _tabLine.Swipe.OnCurrentPageChanged += HandleSwipePageChanged;

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

    private void OnDisable()
    {
        if (_tabLine != null && _tabLine.Swipe != null)
            _tabLine.Swipe.OnCurrentPageChanged -= HandleSwipePageChanged;
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
            ApplyButtonsVisibility();
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

        return true; //Remove this later when we actually want to block unauthorized edits to clanprofile.

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

        if (_membersPageController != null)
        {
            _membersPageController.SetViewedClan(clan.Id);

        }

        // Show correct buttons
        bool isInClan = ServerManager.Instance.Clan != null && clan.Id == ServerManager.Instance.Clan._id;
        _isInClanCached = isInClan;

        _canEditCached = CanCurrentPlayerEditClan(clan);

        if(_editButton != null)
        {
            _editButton.SetActive(_canEditCached);
        }

        ApplyButtonsVisibility();

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

    public void SetCurrentPageToProfile()
    {
        if (!_isInClanCached || _currentPage == ClanPage.Settings) return;
        StopAllCoroutines();

        StopAllCoroutines();
        StartCoroutine(ForceSwipeUIPageAfterLayout(0, instant: true));
    }

    public void SetCurrentPageToMembers()
    {
        if (!_isInClanCached || _currentPage == ClanPage.Settings) return;
        StopAllCoroutines();

        StopAllCoroutines();
        StartCoroutine(ForceSwipeUIPageAfterLayout(1, instant: true));
    }

    public void SetCurrentPageToSettings()
    {
        if (_memberDetailsPopup != null)
            _memberDetailsPopup.Hide();

        ApplyCurrentPage(ClanPage.Settings);

        if (_tabLine?.Swipe != null)
        {
            _tabLine.Swipe.hardBlocked = true;
        }

        ApplyButtonsVisibility();
    }

    public void ExitSettingsToProfile()
    {
        if (_tabLine?.Swipe != null)
            _tabLine.Swipe.hardBlocked = false;

        //Unlocks tab line swiping when exiting settings page
        SetTabLineSwipeLock(false);

        ShowProfilePage();

        StopAllCoroutines();
        StartCoroutine(ForceSwipeUIPageAfterLayout(0, instant: true));
    }

    private void ApplyButtonsVisibility()
    {
        bool isInClan = _isInClanCached;

        // Profile page buttons only visible when profile page is visible
        if (_inClanButtons != null)
            _inClanButtons.SetActive(isInClan && _currentPage == ClanPage.Profile);

        if (_notInClanButtons != null)
            _notInClanButtons.SetActive(!isInClan && _currentPage == ClanPage.Profile);

        // Members page buttons
        if (_clanMemberPageButtons != null)
            _clanMemberPageButtons.SetActive(isInClan && _currentPage == ClanPage.Members);

        // Settings buttons
        if (_editViewButtons != null)
            _editViewButtons.SetActive(_currentPage == ClanPage.Settings);

        // Makes sure edit buttons behave the right way based on profile rights
        if (_editButton != null)
            _editButton.SetActive(_canEditCached);
    }

    private void HandleSwipePageChanged()
    {
        if (_tabLine == null || _tabLine.Swipe == null) return;
        if (_currentPage == ClanPage.Settings) return;

        int page = _tabLine.Swipe.CurrentPage;
        ApplyCurrentPage(page == 0 ? ClanPage.Profile : ClanPage.Members);
    }

    private void ApplyCurrentPage(ClanPage page)
    {
        _currentPage = page;
        ApplyButtonsVisibility();
        RefreshSwipeEnabledState();
    }

    private IEnumerator ForceSwipeUIPageAfterLayout(int pageIndex, bool instant)
    {     
        // Waits for layout + rect.width to settle
        yield return null;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        var swipe = _tabLine != null ? _tabLine.Swipe : null;
        if (swipe == null) yield break;

        swipe.UpdateSwipeAreaValues();
        if (_currentPage != ClanPage.Settings)
            swipe.IsEnabled = true;

        yield return swipe.SetScrollBarValue(pageIndex, instant);

        // Syncs internal state
        ApplyCurrentPage(pageIndex == 0 ? ClanPage.Profile : ClanPage.Members);
    }

    private void RefreshSwipeEnabledState()
    {
        bool isSettings = _currentPage == ClanPage.Settings;
        bool inClan = _isInClanCached;

        bool allowNav = !isSettings && inClan;

        if (_tabLine?.Swipe != null)
        {
            _tabLine.Swipe.IsEnabled = allowNav;
        }

        SetTabButtonsInteractable(allowNav);
    }


    private void Reset()
    {
        _isInClanCached = false;
        _canEditCached = false;

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
        _isInClanCached = isInClan;

        _inClanPanel.SetActive(isInClan);
        _noClanPanel.SetActive(!isInClan);

        if(_clanSwipeRoot != null)
        {
            _clanSwipeRoot.SetActive(isInClan);
        }

        if(_tabLine?.Swipe != null)
        {
            _tabLine.Swipe.hardBlocked = !isInClan;
        }

        RefreshSwipeEnabledState();
    }

    //prevents tab line button interaction when not in clan and in settings page
    private void SetTabButtonsInteractable(bool interactable)
    {
        if (_tabButtonsRoot == null) return;

        var group = _tabButtonsRoot.GetComponent<CanvasGroup>();
        if (group == null) group = _tabButtonsRoot.AddComponent<CanvasGroup>();

        group.interactable = interactable;
        group.blocksRaycasts = interactable;
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
        if (_swipeScrollRect != null)
        {
            _swipeScrollRect.StopMovement();
            _swipeScrollRect.velocity = Vector2.zero;
        }

        ShowSettingsPage();
        SetCurrentPageToSettings();

        if (_tabLine != null)
        {
            _tabLine.SetLockActiveFromSwipe(true);
        }
    }

    public void SetTabLineSwipeLock(bool locked)
    {
        if (_tabLine != null)
        {
            _tabLine.SetLockActiveFromSwipe(locked);
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
