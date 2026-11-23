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

    private readonly struct DropdownEntry
    {
        public readonly string Label;
        public readonly Action<int> OnSelected; // null = divider

        public DropdownEntry(string label, Action<int> onSelected)
        {
            Label = label;
            OnSelected = onSelected;
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
    private const string _divider = "--------";
    private const string _addRoleText = "Add role";
    private ClanData _clanData = null;
    private ClanRoles _draft;
    private ClanRoles _draftRole;
    private ClanRoles _selectedRole;
    private MemberRights _selectedRight;
    private readonly List<DropdownEntry> _entries = new();

    public void OpenPopup()
    {
        popup.SetActive(true);

        currentRoleText.text = _currentMember.Role.name;

        SetNewRoleView(false);

        LoadClanData();
        PopulateDropdown(_clanData);
        SelectCurrentMemberRole();
    }

    public void ClosePopup() {
        CancelNewRole();
        popup.SetActive(false);
    }

    void Start()
    {
        if (CurrentMember.Role.name != null)
        {
            Debug.LogWarning($"No current member seleceted?");
        }
    }
    void OnEnable()
    {
        SetNewRoleView(false);

        roleDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        saveRoleButton.onClick.AddListener(OnStartVoteClicked);
        cancelNewRoleButton.onClick.AddListener(CancelNewRole);
    }

    void OnDisable()
    {
        roleDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        saveRoleButton.onClick.RemoveListener(OnStartVoteClicked);
        cancelNewRoleButton.onClick.RemoveListener(CancelNewRole);
        ClosePopup();
    }

    private void CancelNewRole() {
        SetNewRoleView(false);

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
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

    private void PopulateDropdown(ClanData clan)
    {
        _entries.Clear();

        _entries.Add(new(_addRoleText, _ => NewRole()));
        _entries.Add(new(_divider, null));

        foreach (var role in clan.ClanRoles)
        _entries.Add(ViewEntry(role.name, () => ShowRole(role)));

        _entries.Add(new(_divider, null));

        foreach (MemberRights right in Enum.GetValues(typeof(MemberRights)))
        _entries.Add(ViewEntry(GetDisplayName(right), () => ShowRight(right)));

        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(_entries.Select(e => e.Label).ToList());
        SelectCurrentMemberRole();
    }

    private DropdownEntry ViewEntry(string label, Action show) => new(label, index =>
    {
        _previousValidIndex = index;
        SetNewRoleView(false);
        show();
    });


    private void OnDropdownValueChanged(int index)
    {
        if (index < 0 || index >= _entries.Count)
            return;

        var entry = _entries[index];
        if (entry.OnSelected == null)
            roleDropdown.SetValueWithoutNotify(_previousValidIndex);
        else
            entry.OnSelected(index);
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

        SetNewRoleView(false);

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

        SetNewRoleView(true);
        NewRoleOptions();

        // nameInput.text = "";
        saveRoleButton.interactable = true;
    }

    private void NewRoleOptions()
    {
        _draftRole = new ClanRoles { rights = new ClanRoles.ClanRights() };

        foreach (MemberRights right in Enum.GetValues(typeof(MemberRights)))
        {
            GameObject newTextObj = Instantiate(textPrefab, contentParent);

            TextMeshProUGUI label = newTextObj.GetComponent<TextMeshProUGUI>();
            Button button = newTextObj.GetComponentInChildren<Button>();
            if (label == null || button == null) continue;

            Image buttonImage = button.GetComponent<Image>();
            MemberRights capturedRight = right;

            UpdateVisuals(label, buttonImage, capturedRight);

            button.onClick.AddListener(() =>
            {
                bool newValue = !_draftRole.HasRights(capturedRight);
                SetNewRoleRight(capturedRight, newValue);
                UpdateVisuals(label, buttonImage, capturedRight);
            });
        }
    }

    private void UpdateVisuals(TextMeshProUGUI label, Image buttonImage, MemberRights right)
    {
        bool isAllowed = _draftRole.HasRights(right);
        string color = isAllowed ? "green" : "red";

        label.text = $"{GetDisplayName(right)}: <color={color}>{isAllowed}</color>";
        buttonImage.color = isAllowed ? Color.green : Color.red;
    }

    private void SetNewRoleRight(MemberRights right, bool isAllowed)
    {
        _draftRole.SetRights(right, isAllowed);
    }

    private void SetNewRoleView(bool isNewRole)
    {
        if (newRoleContainer == null || defaultContainer == null) return;

        newRoleContainer.SetActive(isNewRole);
        defaultContainer.SetActive(!isNewRole);
    }

    private static string GetDisplayName(MemberRights right) => right switch
    {
        MemberRights.CanEditHome     => "Edit home",
        MemberRights.CanEditClanData => "Edit clan data",
        MemberRights.CanEditRights   => "Edit rights",
        MemberRights.CanManageRoles  => "Manage roles",
        MemberRights.CanManageShop   => "Manage shop",
        _                            => right.ToString()
    };
}
