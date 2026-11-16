using System;
using System.Collections;
using Altzone.Scripts.Config;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClanMemberRoleRights : MonoBehaviour
{
    private ClanMember _currentMember;
    public ClanMember CurrentMember
    {
        get
        {
            return _currentMember;
        }
        set
        {
            _currentMember = value;
        }
    }

    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject popup;
    [SerializeField] private TextMeshProUGUI currentRoleText;
    [SerializeField] private TMP_Dropdown roleDropdown;
    [SerializeField] private TMP_Dropdown roleRightsDropdown;
    [SerializeField] private Toggle rightAllowedToggle;

    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button saveRoleButton;
    [SerializeField] private Button cancelRoleButton;
    [SerializeField] private GameObject newRoleContainer;


    private ClanData _clanData = null;
    private ClanRoles _draft;
    private ClanRoles _selectedRole;
    private MemberRights _selectedRight;

    public void OpenPopup() => popup.SetActive(true);
    public void ClosePopup() => popup.SetActive(false);

    void Start()
    {
        roleDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        roleDropdown.onValueChanged.AddListener(OnRolerightsDropdownValueChanged);
        saveRoleButton.onClick.AddListener(OnStartVoteClicked);
        newRoleContainer.SetActive(false);

        if (CurrentMember.Role.name != null)
        {
            Debug.LogWarning($"No current member seleceted?");
        }
        LoadClanData();

        PopulateRightsDropdown();
        PopulateRoles(_clanData);
        SelectCurrentMemberRole();
    }

    void OnDestroy()
    {
        roleDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        roleDropdown.onValueChanged.RemoveListener(OnRolerightsDropdownValueChanged);
        saveRoleButton.onClick.RemoveListener(OnStartVoteClicked);
    }

    private void LoadClanData()
    {
        DataStore store = Storefront.Get();
        PlayerData player = null;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player == null) return;

        store.GetClanData(player.ClanId, data => _clanData = data);
        Debug.Log("did a thing");
    }

    private void PopulateRoles(ClanData clan)
    {
        List<string> options = new List<string>();
        foreach (var role in clan.ClanRoles) {
            options.Add(role.name);
        }
        options.Add("Add role");
        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(options);
    }

    private void PopulateRightsDropdown()
    {
        var options = new List<string>();
        foreach (MemberRights right in Enum.GetValues(typeof(MemberRights)))
        {
            options.Add(right.ToString());
        }

        roleRightsDropdown.ClearOptions();
        roleRightsDropdown.AddOptions(options);
    }

    private void SelectCurrentMemberRole()
    {
        int index = _clanData.ClanRoles.FindIndex(r => r.name == CurrentMember.Role.name);
        if (index == -1) return;

        roleDropdown.SetValueWithoutNotify(index);
        roleDropdown.RefreshShownValue();

        ShowRole(_clanData.ClanRoles[index]);
    }

    private void ShowRole(ClanRoles role)
    {
        _selectedRole = role;

        if (currentRoleText != null)
        {
            currentRoleText.text = string.IsNullOrEmpty(role.name) ? "New role" : role.name;
        }

        RefreshRightToggle();
    }

    private void RefreshRightToggle()
    {
        if (_selectedRole == null) return;

        bool isAllowed = _selectedRole.HasRights(_selectedRight);
    }

    private void OnDropdownValueChanged(int index)
    {
        // Last option is new role, if index higher than ClanRoles.Count, treat as new role
        if (index >= _clanData.ClanRoles.Count) {
            NewRole();
            return;
        }
        newRoleContainer.SetActive(false);
        ShowRole(_clanData.ClanRoles[index]);
    }

    private void OnRolerightsDropdownValueChanged(int index)
    {
        if (_selectedRole == null) return;

        // Toggles rights locally for now (for a draft this is what gets sent in the poll)
        Debug.Log($"Right {_selectedRight} for role '{_selectedRole.name}'");

        MemberRights selectedRight = (MemberRights)index;
        Debug.Log($"Selected role is {selectedRight}");
    }


    private void OnButtonClicked(MemberRights rightName, bool allowed, ClanRoles currentRole)
    {
        // Toggles rights locally for now
        Debug.Log($"Clicked button for right: {rightName}, Status: {allowed}");

        currentRole.SetRights(rightName, !allowed);
    }

    public void OnStartVoteClicked() {
        _draft.name = nameInput.text;
        bool? result = null;

        PollManager.CreateRolePoll(_currentMember.Id, _draft, c => result = c);
        newRoleContainer.SetActive(false);
        ClosePopup();
    }

    private void NewRole() {
        _draft = new ClanRoles
        {
            name = "",
            clanRoleType = "named",
            rights = new ClanRoles.ClanRights(),
        };

        newRoleContainer.SetActive(true);
        nameInput.text = "";
        saveRoleButton.interactable = true;
    }
}
