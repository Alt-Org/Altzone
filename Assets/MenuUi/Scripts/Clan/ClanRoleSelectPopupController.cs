using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;

public class ClanRoleSelectPopupController : MonoBehaviour
{
    [Header("Root / Close")]
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _blockerButton;

    [Header("Header")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _currentRoleText;
    [SerializeField] private AvatarFaceLoader _avatarFaceLoader;

    [Header("Role list (inside popup)")]
    [SerializeField] private RectTransform _listContent;
    [SerializeField] private ClanRoleItemTemplate _itemTemplate;
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private ClanRoleCatalog _roleCatalog; 
    [SerializeField] private Sprite _fallbackIcon;         

    [Header("Actions")]
    [SerializeField] private Button _voteButton;

    private readonly List<ClanRoleItemTemplate> _spawned = new();

    private ClanMember _member;
    private ClanRoles _selectedRole;

    private string _currentRoleName;

    private void Awake()
    {
        if (_root == null) _root = gameObject;

        _closeButton?.onClick.AddListener(Hide);
        _blockerButton?.onClick.AddListener(Hide);
        _voteButton?.onClick.AddListener(OnVotePressed);

        if (_toggleGroup == null && _listContent != null)
            _toggleGroup = _listContent.GetComponent<ToggleGroup>();
    }

    private void Start()
    {
        Hide();
    }

    public void Show(ClanMember member, List<ClanRoles> roles)
    {
        if (member == null) return;

        _member = member;
        _selectedRole = null;

        _root.SetActive(true);

        if (_voteButton != null) _voteButton.interactable = false;

        if (_nameText != null) _nameText.text = member.Name ?? "";

        _currentRoleName = member.Role != null ? member.Role.name : "";
        var currentRoleName = _currentRoleName;
        if (_currentRoleText != null)
            _currentRoleText.text = string.IsNullOrEmpty(currentRoleName)
                ? "Nykyinen rooli: —"
                : $"Nykyinen rooli: {GetDisplayName(currentRoleName)}";

        UpdateFace(member);

        BuildRoleList(roles);
    }

    public void Hide()
    {
        ClearList();
        _member = null;
        _selectedRole = null;

        if (_root != null) _root.SetActive(false);
    }

    private void BuildRoleList(List<ClanRoles> roles)
    {
        ClearList();

        if (roles == null || _listContent == null || _itemTemplate == null) return;

        // Single-select
        if (_toggleGroup != null) _toggleGroup.allowSwitchOff = true;

        foreach (var role in roles)
        {
            if (role == null || string.IsNullOrEmpty(role.name)) continue;

            var item = Instantiate(_itemTemplate, _listContent);
            item.gameObject.SetActive(true);

            string displayName = GetDisplayName(role.name);

            Sprite icon = null;
            if (_roleCatalog != null) icon = _roleCatalog.GetIcon(role.name);
            if (icon == null) icon = _fallbackIcon;

            bool isCurrent = !string.IsNullOrEmpty(_currentRoleName) &&
                 string.Equals(role.name, _currentRoleName, StringComparison.OrdinalIgnoreCase);

            // Default: nothing selected when opened
            item.Init(
                roleName: role.name,
                displayName: displayName,
                icon: icon,
                isOn: false,
                onChanged: (roleName, isOn) =>
                {
                    if (!isOn) return;

                    // Extra safety: ignore current role even if it somehow gets toggled
                    if (!string.IsNullOrEmpty(_currentRoleName) &&
                        string.Equals(roleName, _currentRoleName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    _selectedRole = role;
                    if (_voteButton != null) _voteButton.interactable = true;
                },
                toggleGroup: _toggleGroup
            );

            // Disable current role so user must pick a different one
            if (isCurrent)
            {
                displayName += " (Nykyinen)";   
                var t = item.GetComponentInChildren<Toggle>(true);
                if (t != null)
                {
                    t.interactable = false;
                    t.isOn = false;
                }
            }

            _spawned.Add(item);
        }

        if (_toggleGroup != null)
            _toggleGroup.SetAllTogglesOff(true); 

        _selectedRole = null;
        if (_voteButton != null) _voteButton.interactable = false;
    }

    private void ClearList()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null) Destroy(_spawned[i].gameObject);
        }
        _spawned.Clear();
    }

    private void UpdateFace(ClanMember member)
    {
        if (_avatarFaceLoader == null) return;
        if (member.AvatarData == null || AvatarDesignLoader.Instance == null) return;

        var visualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(member.AvatarData);
        if (visualData != null)
            _avatarFaceLoader.UpdateVisuals(visualData);
    }

    private string GetDisplayName(string roleName)
    {
        return _roleCatalog != null ? _roleCatalog.GetDisplayName(roleName) : roleName;
    }

    private void OnVotePressed()
    {
        if (_member == null || _selectedRole == null) return;

        Debug.Log($"Start role vote: member={_member.Name} role={_selectedRole.name} ({_selectedRole._id})");
        Hide();
    }
}
