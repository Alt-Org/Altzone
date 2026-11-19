using System;
using System.Linq;
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
    [SerializeField] private TextMeshProUGUI currentSelectionInfo;
    [SerializeField] private TMP_Dropdown roleDropdown;
    [SerializeField] private Toggle rightAllowedToggle;
    [SerializeField] private TextMeshProUGUI currentRoleText;

    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button saveRoleButton;
    [SerializeField] private Button cancelNewRoleButton;
    [SerializeField] private GameObject newRoleContainer;
    [SerializeField] private GameObject defaultContainer;

    private int _previousValidIndex;
    private string _divider = "--------";
    private ClanData _clanData = null;
    private ClanRoles _draft;
    private ClanRoles _selectedRole;
    private MemberRights _selectedRight;

    public void OpenPopup()
    {
        popup.SetActive(true);

        currentRoleText.text = _currentMember.Role.name;

        defaultContainer.SetActive(true);
        newRoleContainer.SetActive(false);

        LoadClanData();
        PopulateVotes(_clanData);
        SelectCurrentMemberRole();
    }

    public void ClosePopup() => popup.SetActive(false);

    void Start()
    {
        roleDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        saveRoleButton.onClick.AddListener(OnStartVoteClicked);
        cancelNewRoleButton.onClick.AddListener(CancelNewRole);
        newRoleContainer.SetActive(false);

        if (CurrentMember.Role.name != null)
        {
            Debug.LogWarning($"No current member seleceted?");
        }
    }

    void OnUnload()
    {
        roleDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        saveRoleButton.onClick.RemoveListener(OnStartVoteClicked);
        cancelNewRoleButton.onClick.RemoveListener(CancelNewRole);
    }

    private void CancelNewRole() {
        defaultContainer.SetActive(true);
        newRoleContainer.SetActive(false);
        roleDropdown.SetValueWithoutNotify(_previousValidIndex);
    }

    private void LoadClanData()
    {
        if (_clanData != null)
            return;

        DataStore store = Storefront.Get();
        PlayerData player = null;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player == null) return;

        store.GetClanData(player.ClanId, data => _clanData = data);
    }

    private void PopulateVotes(ClanData clan)
    {
        List<string> options = new List<string>();
        foreach (var role in clan.ClanRoles)
        {
            options.Add(role.name);
        }
        options.Add("Add role");
        options.Add(_divider);

        foreach (MemberRights right in Enum.GetValues(typeof(MemberRights)))
        {
            options.Add(GetDisplayName(right));
        }

        SelectCurrentMemberRole();
        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(options);
    }

    private void SelectCurrentMemberRole()
    {
        int index = _clanData.ClanRoles.FindIndex(r => r.name == CurrentMember.Role.name);
        _previousValidIndex = index;
        if (index == -1) return;

        roleDropdown.SetValueWithoutNotify(index);
        roleDropdown.RefreshShownValue();

        ShowRole(_clanData.ClanRoles[index]);
    }

    private void ShowRight(MemberRights right)
    {
        _selectedRight = right;
        _selectedRole = _currentMember.Role;

        Debug.Log($"_selectedRight = {right}");

        bool isAllowed = _selectedRole.HasRights(_selectedRight);

        if (currentSelectionInfo != null)
        {
            currentSelectionInfo.text = $"{GetDisplayName(right)} is {isAllowed}";
        }
    }

    private void ShowRole(ClanRoles role)
    {
        if (currentSelectionInfo == null)
            return;

        _selectedRole = role;

        var allowedRights = Enum.GetValues(typeof(MemberRights))
            .Cast<MemberRights>()
            .Where(right => _selectedRole.HasRights(right))
            .Select(right => GetDisplayName(right))
            .ToList();

        currentSelectionInfo.text = $"Currently existing rights for {_selectedRole.name}:\n"
                                    + (allowedRights.Count > 0
                                        ? $"- {string.Join("\n- ", allowedRights)}"
                                        : $"none");
    }

    private void RefreshRightToggle()
    {
        if (_selectedRole == null) return;

        bool isAllowed = _selectedRole.HasRights(_selectedRight);
        Debug.Log($"Right is or is not allowed? {isAllowed}");
    }

    private void OnDropdownValueChanged(int index)
    {
        if (index == _clanData.ClanRoles.Count)
        {
            NewRole();
            return;
        }

        if (roleDropdown.options[index].text == _divider)
        {
            roleDropdown.SetValueWithoutNotify(_previousValidIndex);
            return;
        }

        _previousValidIndex = index;

        if (index > _clanData.ClanRoles.Count + 1)
        {
            // +2, we need to count the divider too
            ShowRight((MemberRights)index - (_clanData.ClanRoles.Count + 2));
            return;
        }

        newRoleContainer.SetActive(false);
        defaultContainer.SetActive(true);
        ShowRole(_clanData.ClanRoles[index]);
    }

    private void OnButtonClicked(MemberRights rightName, bool allowed, ClanRoles currentRole)
    {
        // Toggles rights locally for now
        Debug.Log($"Clicked button for right: {rightName}, Status: {allowed}");

        currentRole.SetRights(rightName, !allowed);
    }

    public void OnStartVoteClicked()
    {
        _draft.name = nameInput.text;
        bool? result = null;

        PollManager.CreateRolePoll(_currentMember.Id, _draft, c => result = c);
        newRoleContainer.SetActive(false);
        ClosePopup();
    }

    private void NewRole()
    {
        _draft = new ClanRoles
        {
            name = "",
            clanRoleType = "named",
            rights = new ClanRoles.ClanRights(),
        };

        newRoleContainer.SetActive(true);
        defaultContainer.SetActive(false);

        // nameInput.text = "";
        saveRoleButton.interactable = true;
    }

    public static string GetDisplayName(MemberRights right) => right switch
    {
        MemberRights.CanEditHome     => "Edit home",
        MemberRights.CanEditClanData => "Edit clan data",
        MemberRights.CanEditRights   => "Edit rights",
        MemberRights.CanManageRoles  => "Manage roles",
        MemberRights.CanManageShop   => "Manage shop",
        _                            => right.ToString()
    };
}
