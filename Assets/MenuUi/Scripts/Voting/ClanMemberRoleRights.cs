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
        public readonly object Tag;
        public readonly Action<int> OnSelected; // null = divider

        public DropdownEntry(string label, object tag, Action<int> onSelected)
        {
            Label = label;
            Tag = tag;
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
    private ClanRoles _selectedRole;
    private MemberRights _selectedRight;
    private readonly List<DropdownEntry> _entries = new();

    public void OpenPopup()
    {
        if (_currentMember?.Role == null)
        {
            Debug.LogWarning("No current member or role selected");
            return;
        }
        popup.SetActive(true);
        currentRoleText.text = _currentMember.Role.name;

        LoadClanData(clan =>
        {
            _clanData = clan;
            PopulateDropdown(clan);
        });
    }

    public void ClosePopup() {
        CancelNewRole();
        popup.SetActive(false);
    }

    void Start()
    {
        if (CurrentMember.Role.name == null)
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
        _draft = null;
    }

    private void CancelNewRole() {
        SetNewRoleView(false);

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        roleDropdown.SetValueWithoutNotify(_previousValidIndex);
        nameInput.text = "";
        _draft = null;
    }

    private void LoadClanData(Action<ClanData> onLoaded)
    {
        DataStore store = Storefront.Get();
        PlayerData player = null;

        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player == null) return;

        store.GetClanData(player.ClanId, clan =>
        {
            if (clan != null)
                onLoaded(clan);
        });
    }

    private void PopulateDropdown(ClanData clan)
    {
        _entries.Clear();

        _entries.Add(new(_addRoleText, null, _ => NewRole()));
        _entries.Add(new(_divider, null, null));

        foreach (var role in clan.ClanRoles)
            _entries.Add(ViewEntry(role.name, role, () => ShowRole(role)));

        _entries.Add(new(_divider, null, null));

        foreach (MemberRights right in Enum.GetValues(typeof(MemberRights)))
            _entries.Add(ViewEntry(GetDisplayName(right), right, () => ShowRight(right)));

        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(_entries.Select(e => e.Label).ToList());
        SelectCurrentMemberRole();
    }

    private DropdownEntry ViewEntry(string label, object tag, Action show) => new(label, tag, index =>
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
        int index = _entries.FindIndex(e => e.Tag is ClanRoles r && r.name == CurrentMember.Role.name);
        if (index == -1) return;
        _previousValidIndex = index;

        roleDropdown.SetValueWithoutNotify(index);
        roleDropdown.RefreshShownValue();
        _entries[index].OnSelected(index);
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
        _selectedRole = role;

        if (currentSelectionInfo == null)
            return;

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
        if (_draft == null) return;

        string roleName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(roleName))
        {
            Debug.LogWarning("Role name is empty");
            return;
        }
        if (_clanData.ClanRoles.Any(r => r.name == roleName))
        {
            Debug.LogWarning($"Role '{roleName}' already exists");
            return;
        }

        _draft.name = roleName;
        PollManager.CreateRolePoll(_currentMember.Id, _draft, success =>
        {
            if (!success)
                Debug.LogWarning("Creating role poll failed");
        });

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
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

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
                bool newValue = !_draft.HasRights(capturedRight);
                SetNewRoleRight(capturedRight, newValue);
                UpdateVisuals(label, buttonImage, capturedRight);
            });
        }
    }

    private void UpdateVisuals(TextMeshProUGUI label, Image buttonImage, MemberRights right)
    {
        bool isAllowed = _draft.HasRights(right);
        string color = isAllowed ? "green" : "red";

        label.text = $"{GetDisplayName(right)}: <color={color}>{isAllowed}</color>";
        buttonImage.color = isAllowed ? Color.green : Color.red;
    }

    private void SetNewRoleRight(MemberRights right, bool isAllowed)
    {
        _draft.SetRights(right, isAllowed);
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
