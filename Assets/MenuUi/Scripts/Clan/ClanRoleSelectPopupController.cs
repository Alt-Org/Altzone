using System.Collections.Generic;
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
    [SerializeField] private Button _itemTemplate;         
    [SerializeField] private ClanRoleCatalog _roleCatalog; 
    [SerializeField] private Sprite _fallbackIcon;         

    [Header("Actions")]
    [SerializeField] private Button _voteButton;

    [Header("Selection Highlight")]
    [SerializeField] private string _selectedHighlightName = "SelectedHighlight";

    private readonly Dictionary<Button, GameObject> _highlightByButton = new();

    private readonly List<Button> _spawned = new();
    private ClanMember _member;
    private ClanRoles _selectedRole;

    private void Awake()
    {
        if (_root == null) _root = gameObject;

        _closeButton?.onClick.AddListener(Hide);
        _blockerButton?.onClick.AddListener(Hide);
        _voteButton?.onClick.AddListener(OnVotePressed);

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

        var currentRoleName = member.Role != null ? member.Role.name : "";
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
        _highlightByButton.Clear();

        if (roles == null || _listContent == null || _itemTemplate == null) return;

        foreach (var role in roles)
        {
            var btn = Instantiate(_itemTemplate, _listContent);
            btn.gameObject.SetActive(true);

            // Text
            var tmp = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) tmp.text = GetDisplayName(role.name);

            // Icon
            var iconImg = btn.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImg != null)
            {
                var icon = _roleCatalog != null ? _roleCatalog.GetIcon(role.name) : null;
                iconImg.sprite = icon != null ? icon : _fallbackIcon;
            }

            // Highlight marker
            var highlight = btn.transform.Find(_selectedHighlightName)?.gameObject;
            if (highlight != null) highlight.SetActive(false);
            _highlightByButton[btn] = highlight;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                SetSelected(btn, role);
            });

            _spawned.Add(btn);
        }
    }

    private void SetSelected(Button selectedButton, ClanRoles role)
    {
        _selectedRole = role;

        // turn off all highlights
        foreach (var b in _spawned)
        {
            if (b == null) continue;
            if (_highlightByButton.TryGetValue(b, out var h) && h != null)
                h.SetActive(false);
        }

        // turn on selected highlight
        if (selectedButton != null && _highlightByButton.TryGetValue(selectedButton, out var selectedH) && selectedH != null)
            selectedH.SetActive(true);

        if (_voteButton != null)
            _voteButton.interactable = true;
    }

    private void ClearList()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null) Destroy(_spawned[i].gameObject);
        }
        _spawned.Clear();
        _highlightByButton.Clear();
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
