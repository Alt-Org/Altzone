using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Window;
using MenuUi.Scripts.SwipeNavigation;
using MenuUi.Scripts.TabLine;
using MenuUi.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI _clanAgeText;

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
    [SerializeField] private Button _editButton;
    [SerializeField] private GameObject _viewClansButton;
    [SerializeField] private Button _membersFilterButton;
    [SerializeField] private Button _popupOverlayButton;
    [SerializeField] private ClanSearchNavigator _clanSearchNavigator;

    private bool _isInClanCached;
    private bool _canEditCached;
    private bool _hasClanViewedCached;

    private bool _isProfilePageVisible = true;

    [Header("Rules")]
    [SerializeField] private Button _rulesButton;
    [SerializeField] private GameObject _rulesPopup;
    [SerializeField] private GameObject _openBookObject;
    [SerializeField] private GameObject _closedBookObject;
    [SerializeField] private Button _rulesPopupCloseButton;

    [Header("Carbon emission popup")]
    [SerializeField] private Button _carbonEmissionButton;
    [SerializeField] private GameObject _carbonEmissionPopup;
    [SerializeField] private Button _carbonEmissionPopupCloseButton;

    [Header("pop ups")]
    [SerializeField] private ClanSearchPopup _clanPopup;
    [SerializeField] private GameObject _overlay;
    [SerializeField] private ClanConfirmPopup _confirmPopup;
    [SerializeField] private ClanMemberPopupController _memberDetailsPopup;
    [SerializeField] private ClanMembersFiltersPopup _membersFiltersPopup;

    [SerializeField] private GameObject _passwordPopup;
    [SerializeField] private Button _clanLockButton;
    [SerializeField] private Button _passwordPopupContinueButton;

    [SerializeField] private ClanAddFriendPopupController _addFriendPopup;

    [Header("Leave clan hold popup")]
    [SerializeField] private GameObject _leaveClanHoldPopup;
    [SerializeField] private HoldToLeaveClanButton _holdLeaveClanButton;
    [SerializeField] private Button _leaveClanHoldCancelButton;
    [SerializeField] private Button _leaveClanHoldCloseButton;

    private bool _isCurrentClanLocked;

    private string _currentViewedClanId;
    private bool _filtersWired;

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

    private string GetAgeText(ClanAge age)
    {
        switch (age)
        {
            case ClanAge.None:
                return "None";

            case ClanAge.Teenagers:
                return "Teinit";

            case ClanAge.Toddlers:
                return "Lapset";

            case ClanAge.Adults:
                return "Aikuiset";

            case ClanAge.Elderly:
                return "Vanhukset";

            case ClanAge.All:
                return "Kaikenikäiset";

            default:
                return string.Empty;
        }
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

        if (_editButton != null)
        {
            _editButton.onClick.RemoveListener(OnClickEditClanSettings);
            _editButton.onClick.AddListener(OnClickEditClanSettings);
        }

        OpenLink();

        if (_carbonEmissionButton != null)
        {
            _carbonEmissionButton.onClick.RemoveListener(OpenCarbonEmissionPopup);
            _carbonEmissionButton.onClick.AddListener(OpenCarbonEmissionPopup);
        }

        if (_carbonEmissionPopupCloseButton != null)
        {
            _carbonEmissionPopupCloseButton.onClick.RemoveListener(CloseCarbonEmissionPopup);
            _carbonEmissionPopupCloseButton.onClick.AddListener(CloseCarbonEmissionPopup);
        }

        CloseCarbonEmissionPopup();

        if (_rulesButton != null)
        {
            _rulesButton.onClick.RemoveListener(OpenRulesPopup);
            _rulesButton.onClick.AddListener(OpenRulesPopup);
        }

        if (_rulesPopupCloseButton != null)
        {
            _rulesPopupCloseButton.onClick.RemoveListener(CloseRulesPopup);
            _rulesPopupCloseButton.onClick.AddListener(CloseRulesPopup);
        }

        CloseRulesPopup();

        if (_holdLeaveClanButton != null)
        {
            _holdLeaveClanButton.OnHoldCompleted -= OnLeaveClanHoldCompleted;
            _holdLeaveClanButton.OnHoldCompleted += OnLeaveClanHoldCompleted;
        }

        if (_leaveClanHoldCancelButton != null)
        {
            _leaveClanHoldCancelButton.onClick.RemoveListener(CloseLeaveClanHoldPopup);
            _leaveClanHoldCancelButton.onClick.AddListener(CloseLeaveClanHoldPopup);
        }

        if (_leaveClanHoldCloseButton != null)
        {
            _leaveClanHoldCloseButton.onClick.RemoveListener(CloseLeaveClanHoldPopup);
            _leaveClanHoldCloseButton.onClick.AddListener(CloseLeaveClanHoldPopup);
        }

        CloseLeaveClanHoldPopup();

        if (_clanLockButton != null)
        {
            _clanLockButton.onClick.RemoveListener(OnClickClanLock);
            _clanLockButton.onClick.AddListener(OnClickClanLock);
        }

        if (_passwordPopupContinueButton != null)
        {
            _passwordPopupContinueButton.onClick.RemoveListener(ClosePasswordPopup);
            _passwordPopupContinueButton.onClick.AddListener(ClosePasswordPopup);
        }

        if (_passwordPopup != null)
        {
            _passwordPopup.SetActive(false);
        }

        ServerClan clan = DataCarrier.GetData<ServerClan>(DataCarrier.ClanListing, suppressWarning: true);

        string ownClanId = null;

        if (ServerManager.Instance.Clan != null && !string.IsNullOrEmpty(ServerManager.Instance.Clan._id))
        {
            ownClanId = ServerManager.Instance.Clan._id;
        }
        else if (ServerManager.Instance.Player != null && !string.IsNullOrEmpty(ServerManager.Instance.Player.clan_id))
        {
            ownClanId = ServerManager.Instance.Player.clan_id;
        }

        // If DataCarrier contains the player's own clan, don't treat it as another clan.
        if (clan != null && !string.IsNullOrEmpty(ownClanId) && clan._id == ownClanId)
        {
            clan = null;
        }

        if (!string.IsNullOrEmpty(ownClanId))
        {
            _clanHeart.SetOwnClanHeart = true;

            Storefront.Get().GetClanData(ownClanId, (clanData) =>
            {

                if (clanData == null)
                {
                    ShowNoClanState();
                    return;
                }

                SetClanProfile(clanData);
                ResetSwipeToProfileOnOpen();
            });

            if (_leaveClanButton != null)
            {
                _leaveClanButton.onClick.RemoveAllListeners();
                _leaveClanButton.onClick.AddListener(ShowLeaveClanPopUp);
            }
        }
        else if (clan != null)
        {
            _clanHeart.SetOwnClanHeart = false;

            SetClanProfile(new ClanData(clan));

            if (_joinClanButton != null)
            {
                _joinClanButton.onClick.RemoveAllListeners();
                _joinClanButton.onClick.AddListener(() => { ShowClanPopup(clan); });
            }

            ResetSwipeToProfileOnOpen();
        }
        else
        {
            _clanHeart.SetOwnClanHeart = false;
            ShowNoClanState();
        }

        WireMembersFilterButton();
        ForceCloseMembersFiltersPopup();
    }

    private void WireMembersFilterButton()
    {
        if (_filtersWired) return;
        _filtersWired = true;

        if (_membersFilterButton == null || _membersFiltersPopup == null) return;

        _membersFilterButton.onClick.RemoveAllListeners();
        _membersFilterButton.onClick.AddListener(OpenMembersFiltersPopup);

        // Close overlay
        _membersFiltersPopup.Closed -= HandleFiltersClosed;
        _membersFiltersPopup.Closed += HandleFiltersClosed;

        _membersFiltersPopup.OnFiltersChanged -= HandleMembersFiltersChanged;
        _membersFiltersPopup.OnFiltersChanged += HandleMembersFiltersChanged;
    }

    private void HandleMembersFiltersChanged(ClanMembersFiltersPopup.MemberListFilters filters)
    {
        if (_membersPageController == null) return;

        _membersPageController.ApplyFilters(filters.memberSort, filters.selectedRoles);
    }

    private void OpenMembersFiltersPopup()
    {
        if (_memberDetailsPopup != null)
            _memberDetailsPopup.Hide();

        if (_membersFiltersPopup == null) return;

        ShowOverlay(true);

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(CloseMembersFiltersPopup);
            _popupOverlayButton.onClick.AddListener(CloseMembersFiltersPopup);
        }

        // Opens filter popup and searches roles from the server
        _membersFiltersPopup.OpenForClan(_currentViewedClanId);
    }

    private void CloseMembersFiltersPopup()
    {
        if (_membersFiltersPopup != null)
            _membersFiltersPopup.CloseWithoutApplying();

        HandleFiltersClosed();
    }

    private void HandleFiltersClosed()
    {
        if (_popupOverlayButton != null)
            _popupOverlayButton.onClick.RemoveListener(CloseMembersFiltersPopup);

        ShowOverlay(false);
    }


    private void OnDisable()
    {
        if (_tabLine != null && _tabLine.Swipe != null)
            _tabLine.Swipe.OnCurrentPageChanged -= HandleSwipePageChanged;

        if (_editButton != null)
            _editButton.onClick.RemoveListener(OnClickEditClanSettings);

        if (_clanLockButton != null)
            _clanLockButton.onClick.RemoveListener(OnClickClanLock);

        if (_passwordPopupContinueButton != null)
            _passwordPopupContinueButton.onClick.RemoveListener(ClosePasswordPopup);

        if (_rulesButton != null)
            _rulesButton.onClick.RemoveListener(OpenRulesPopup);


        if (_rulesPopupCloseButton != null)
            _rulesPopupCloseButton.onClick.RemoveListener(CloseRulesPopup);

        if (_carbonEmissionButton != null)
            _carbonEmissionButton.onClick.RemoveListener(OpenCarbonEmissionPopup);

        if (_carbonEmissionPopupCloseButton != null)
            _carbonEmissionPopupCloseButton.onClick.RemoveListener(CloseCarbonEmissionPopup);

        if (_holdLeaveClanButton != null)
            _holdLeaveClanButton.OnHoldCompleted -= OnLeaveClanHoldCompleted;

        if (_leaveClanHoldCancelButton != null)
            _leaveClanHoldCancelButton.onClick.RemoveListener(CloseLeaveClanHoldPopup);

        if (_leaveClanHoldCloseButton != null)
            _leaveClanHoldCloseButton.onClick.RemoveListener(CloseLeaveClanHoldPopup);

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(ClosePasswordPopup);
            _popupOverlayButton.onClick.RemoveListener(CloseRulesPopup);
            _popupOverlayButton.onClick.RemoveListener(CloseCarbonEmissionPopup);
            _popupOverlayButton.onClick.RemoveListener(CloseLeaveClanHoldPopup);
        }

        if (_membersFiltersPopup != null)
        {
            _membersFiltersPopup.Closed -= HandleFiltersClosed;
            _membersFiltersPopup.OnFiltersChanged -= HandleMembersFiltersChanged;
        }

        _filtersWired = false;

        if (_memberDetailsPopup != null)
        {
            _memberDetailsPopup.Hide();
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
            _inClanButtons.SetActive(false);
        }

        if (_notInClanButtons != null)
        {
            _notInClanButtons.SetActive(false);
        }

        if (_clanMemberPageButtons != null)
        {
            _clanMemberPageButtons.SetActive(false);
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

    private void ShowNoClanState()
    {
        _isInClanCached = false;
        _canEditCached = false;
        _hasClanViewedCached = false;
        _currentPage = ClanPage.Profile;

        ToggleClanPanel(false);

        if (_inClanButtons != null)
            _inClanButtons.SetActive(false);

        if (_clanMemberPageButtons != null)
            _clanMemberPageButtons.SetActive(false);

        if (_editViewButtons != null)
            _editViewButtons.SetActive(false);

        if (_notInClanButtons != null)
            _notInClanButtons.SetActive(true);

        if (_joinClanButton != null)
            _joinClanButton.gameObject.SetActive(false);

        ShowOverlay(false);
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
        if (clan == null) return;

        _currentViewedClanId = clan.Id;

        bool isOwnClanByServerClan =
    ServerManager.Instance.Clan != null &&
    !string.IsNullOrEmpty(ServerManager.Instance.Clan._id) &&
    clan.Id == ServerManager.Instance.Clan._id;

        bool isOwnClanByPlayer =
            ServerManager.Instance.Player != null &&
            !string.IsNullOrEmpty(ServerManager.Instance.Player.clan_id) &&
            clan.Id == ServerManager.Instance.Player.clan_id;

        bool isInClan = isOwnClanByServerClan || isOwnClanByPlayer;

        _isInClanCached = isInClan;
        _hasClanViewedCached = true;
        _currentPage = ClanPage.Profile;
        _canEditCached = CanCurrentPlayerEditClan(clan);

        ToggleClanPanel(true);

        if (_membersPageController != null)
        {
            _membersPageController.SetViewedClan(clan.Id);
        }

        ApplyButtonsVisibility();

        if (_joinClanButton != null)
        {
            // Show join button only if not in clan
            _joinClanButton.gameObject.SetActive(!isInClan);
            _joinClanButton.interactable = clan.IsOpen && !isInClan;
        }

        // Show clan profile data
        _clanName.text = clan.Name;
        _clanMembers.text = clan.Members.Count + "/30";
        _clanPhrase.text = string.IsNullOrWhiteSpace(clan.Phrase) ? "Klaanilla ei ole mottoa" : clan.Phrase;
        _flagImage.SetFlag(clan.Language);
        _clanGoal.text = ClanDataTypeConverter.GetGoalText(clan.Goals);

        var ageSprite = GetAgeSprite(clan.ClanAge);

        if (_clanAgeImage != null)
        {
            _clanAgeImage.sprite = ageSprite;
            _clanAgeImage.preserveAspect = true;
            _clanAgeImage.enabled = ageSprite != null;
        }

        if (_clanAgeText != null)
        {
            _clanAgeText.text = GetAgeText(clan.ClanAge);
        }

        _valuePanel.SetValues(clan.Values);

        _clanOpenObject.SetActive(clan.IsOpen);
        _clanLockedObject.SetActive(!clan.IsOpen);
        _isCurrentClanLocked = !clan.IsOpen;

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
            _tabLine.Swipe.IsEnabled = false;
            _tabLine.Swipe.ToggleScrollRect(false);
        }

        ApplyButtonsVisibility();
    }

    public void ExitSettingsToProfile()
    {
        if (_tabLine?.Swipe != null)
        {
            _tabLine.Swipe.hardBlocked = false;
            _tabLine.Swipe.IsEnabled = true;
        }

        SetTabLineSwipeLock(false);
        ShowProfilePage();

        StopAllCoroutines();
        StartCoroutine(ForceSwipeUIPageAfterLayout(0, instant: true));
    }

    private void ApplyButtonsVisibility()
    {
        bool isInClan = _isInClanCached;
        bool hasClanView = _hasClanViewedCached;

        if (_inClanButtons != null)
            _inClanButtons.SetActive(isInClan && _currentPage == ClanPage.Profile);

        if (_notInClanButtons != null)
            _notInClanButtons.SetActive(!isInClan && hasClanView && _currentPage == ClanPage.Profile);

        if (_clanMemberPageButtons != null)
            _clanMemberPageButtons.SetActive(isInClan && _currentPage == ClanPage.Members);

        if (_editViewButtons != null)
            _editViewButtons.SetActive(_currentPage == ClanPage.Settings);

        if (_editButton != null)
            _editButton.gameObject.SetActive(isInClan && _canEditCached && _currentPage == ClanPage.Profile);
    }

    private void ResetSwipeToProfileOnOpen()
    {
        // Don't do anything if swipe root isn't shown (e.g. NoClan)
        if (_clanSwipeRoot != null && !_clanSwipeRoot.activeInHierarchy) return;

        // Settings should never be the first view when opening ClanMainView
        _currentPage = ClanPage.Profile;

        // Make sure settings isn't active (extra safety)
        if (_clanSettings != null) _clanSettings.SetActive(false);

        ApplyButtonsVisibility();

        // Force scroll position + internal SwipeUI state to page 0
        StopAllCoroutines();
        StartCoroutine(ForceSwipeUIPageAfterLayout(0, instant: true));
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
        if (page != ClanPage.Members && _memberDetailsPopup != null)
        {
            _memberDetailsPopup.Hide();
        }

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
        bool allowNav = !isSettings && _hasClanViewedCached && _isInClanCached;

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
        _hasClanViewedCached = false;

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

        if (_clanAgeText != null)
        {
            _clanAgeText.text = string.Empty;
        }
    }

    private void ToggleClanPanel(bool showClanPanel)
    {
        _hasClanViewedCached = showClanPanel;

        if (_inClanPanel != null)
            _inClanPanel.SetActive(showClanPanel);

        if (_noClanPanel != null)
            _noClanPanel.SetActive(!showClanPanel);

        if (_clanSwipeRoot != null)
            _clanSwipeRoot.SetActive(showClanPanel);

        if (_tabLine?.Swipe != null)
        {
            _tabLine.Swipe.hardBlocked = !showClanPanel;
            _tabLine.Swipe.IsEnabled = showClanPanel;
        }

        if (showClanPanel)
            SetTabLineSwipeLock(false);

        RefreshSwipeEnabledState();
    }

    //prevents tab line button interaction when not in clan and in settings page
    private void SetTabButtonsInteractable(bool interactable)
    {
        if (_tabButtonsRoot == null) return;

        var group = _tabButtonsRoot.GetComponent<CanvasGroup>();
        if (group == null)
            group = _tabButtonsRoot.AddComponent<CanvasGroup>();

        group.alpha = 1f;
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
            if (!success)
            {
                ShowOverlay(false);
                return;
            }

            ServerManager.Instance.RaiseClanChangedEvent();

            Reset();

            NavigateToClanSearch();
        }));
    }

    private void NavigateToClanSearch()
    {
        if (_clanSearchNavigator == null)
        {
            return;
        }

        _clanSearchNavigator.NavigateToClanSearch();
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

        if (_overlay != null)
        {
            _overlay.SetActive(false);
        }
    }

    public void ShowSettingsPage()
    {
        if (_inClanPanel != null)
        {
            _inClanPanel.SetActive(true);
        }

        if (_overlay != null)
        {
            _overlay.SetActive(true);
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

        if (_memberDetailsPopup != null)
        {
            _memberDetailsPopup.Hide();
        }

        ShowSettingsPage();

        if (_tabLine?.Swipe != null)
        {
            _tabLine.Swipe.hardBlocked = true;
            _tabLine.Swipe.IsEnabled = false;
            _tabLine.Swipe.ToggleScrollRect(false);
        }

        SetTabLineSwipeLock(true);

        ApplyButtonsVisibility();
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
        if (_memberDetailsPopup != null)
            _memberDetailsPopup.Hide();

        ShowOverlay(true);

        if (_leaveClanHoldPopup != null)
            _leaveClanHoldPopup.SetActive(true);

        if (_holdLeaveClanButton != null)
            _holdLeaveClanButton.ResetHold();

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(CloseLeaveClanHoldPopup);
            _popupOverlayButton.onClick.AddListener(CloseLeaveClanHoldPopup);
        }
    }

    private void CloseLeaveClanHoldPopup()
    {
        if (_leaveClanHoldPopup != null)
            _leaveClanHoldPopup.SetActive(false);

        if (_holdLeaveClanButton != null)
            _holdLeaveClanButton.ResetHold();

        if (_popupOverlayButton != null)
            _popupOverlayButton.onClick.RemoveListener(CloseLeaveClanHoldPopup);

        ShowOverlay(false);
    }

    private void OnLeaveClanHoldCompleted()
    {
        CloseLeaveClanHoldPopup();
        LeaveClan();
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

    private void OnClickClanLock()
    {
        if (!_isCurrentClanLocked) return;

        OpenPasswordPopup();
    }

    private void OpenPasswordPopup()
    {
        if (_passwordPopup == null) return;

        ShowOverlay(true);
        _passwordPopup.SetActive(true);

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(ClosePasswordPopup);
            _popupOverlayButton.onClick.AddListener(ClosePasswordPopup);
        }
    }

    private void ClosePasswordPopup()
    {
        if (_passwordPopup != null)
        {
            _passwordPopup.SetActive(false);
        }

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(ClosePasswordPopup);
        }

        ShowOverlay(false);
    }

    private void OpenRulesPopup()
    {
        if (_memberDetailsPopup != null)
            _memberDetailsPopup.Hide();

        if (_rulesPopup != null)
        {
            _rulesPopup.SetActive(true);
        }

        if (_openBookObject != null)
        {
            _openBookObject.SetActive(true);
        }

        if (_closedBookObject != null)
        {
            _closedBookObject.SetActive(false);
        }

        ShowOverlay(true);

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(CloseRulesPopup);
            _popupOverlayButton.onClick.AddListener(CloseRulesPopup);
        }
    }

    private void CloseRulesPopup()
    {
        if (_rulesPopup != null)
        {
            _rulesPopup.SetActive(false);
        }

        if (_openBookObject != null)
        {
            _openBookObject.SetActive(false);
        }

        if (_closedBookObject != null)
        {
            _closedBookObject.SetActive(true);
        }

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(CloseRulesPopup);
        }

        ShowOverlay(false);
    }

    private void OpenCarbonEmissionPopup()
    {
        if (_memberDetailsPopup != null)
            _memberDetailsPopup.Hide();

        if (_carbonEmissionPopup != null)
        {
            _carbonEmissionPopup.SetActive(true);
        }

        ShowOverlay(true);

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(CloseCarbonEmissionPopup);
            _popupOverlayButton.onClick.AddListener(CloseCarbonEmissionPopup);
        }
    }

    private void CloseCarbonEmissionPopup()
    {
        if (_carbonEmissionPopup != null)
        {
            _carbonEmissionPopup.SetActive(false);
        }

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(CloseCarbonEmissionPopup);
        }

        ShowOverlay(false);
    }

    private void ForceCloseMembersFiltersPopup()
    {
        if (_membersFiltersPopup != null)
        {
            _membersFiltersPopup.Hide();
        }

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(CloseMembersFiltersPopup);
        }

        ShowOverlay(false);
    }

    public void OpenAddFriendPopup(ClanMember member)
    {
        if (_addFriendPopup == null) return;

        ShowOverlay(true);

        _addFriendPopup.Show(member);

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(CloseAddFriendPopup);
            _popupOverlayButton.onClick.AddListener(CloseAddFriendPopup);
        }
    }

    private void CloseAddFriendPopup()
    {
        if (_addFriendPopup != null)
        {
            _addFriendPopup.Hide();
        }

        if (_popupOverlayButton != null)
        {
            _popupOverlayButton.onClick.RemoveListener(CloseAddFriendPopup);
        }

        ShowOverlay(false);
    }
}
