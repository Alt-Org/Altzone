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

    private int _previousValidIndex;
    private string _divider = "--------";


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

    public void OpenPopup()
    {
        popup.SetActive(true);

        LoadClanData();
        PopulateVotes(_clanData);
        SelectCurrentMemberRole();
    }
    public void ClosePopup() => popup.SetActive(false);

    void Start()
    {
        roleDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        saveRoleButton.onClick.AddListener(OnStartVoteClicked);
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
        foreach (var role in clan.ClanRoles) {
            options.Add(role.name);
        }
        options.Add("Add role");
        options.Add(_divider);

        foreach (MemberRights right in Enum.GetValues(typeof(MemberRights)))
        {
            options.Add(right.ToString());
        }

        SelectCurrentMemberRole();
        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(options);
    }

    private void SelectCurrentMemberRole()
    {
        int index = _clanData.ClanRoles.FindIndex(r => r.name == CurrentMember.Role.name);
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

        if (currentRoleText != null)
        {
            currentRoleText.text = $"{right} is {isAllowed}";
        }
    }

    private void ShowRole(ClanRoles role)
    {
        _selectedRole = role;

        if (currentRoleText != null)
        {
            currentRoleText.text = "Currently existing rights for role:";
            foreach (MemberRights right in Enum.GetValues(typeof(MemberRights)))
            {
                bool isAllowed = _selectedRole.HasRights(right);
                if (isAllowed)
                    currentRoleText.text += "\n" + string.Join("\n", right);
            }

        }
    }

    private void RefreshRightToggle()
    {
        if (_selectedRole == null) return;

        bool isAllowed = _selectedRole.HasRights(_selectedRight);
        Debug.Log($"Right is or is not allowed? {isAllowed}");
    }

    private void OnDropdownValueChanged(int index)
    {
        if (roleDropdown.options[index].text == _divider)
        {
            roleDropdown.SetValueWithoutNotify(_previousValidIndex);
        }
        else
        {
            _previousValidIndex = index;
        }

        if (index == _clanData.ClanRoles.Count)
        {
            NewRole();
            return;
        }
        else if (index > _clanData.ClanRoles.Count + 1)
        {
            // +2, we need to count the divider too
            ShowRight((MemberRights)index - (_clanData.ClanRoles.Count + 2));
            return;
        }

        newRoleContainer.SetActive(false);
        ShowRole(_clanData.ClanRoles[index]);
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
