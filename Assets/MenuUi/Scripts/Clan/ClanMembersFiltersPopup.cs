using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts;

public class ClanMembersFiltersPopup : MonoBehaviour
{
    public enum MemberSort
    {
        MostWins,
        OldestMemberFirst,
        NewestMemberFirst,
        LeastWins,
        NameAscending,
        NameDescending
    }

    [Serializable]
    public struct MemberListFilters
    {
        public MemberSort memberSort;
        public List<string> selectedRoles;
    }

    [Header("Sort Dropdown")]
    [SerializeField] private TMP_Dropdown _sortDropdown;

    [Header("Roles selection (multi-select list)")]
    [SerializeField] private Transform _rolesContent;
    [SerializeField] private ClanRoleItemTemplate _roleItemPrefab;

    [Header("All roles")]
    [SerializeField] private Toggle _allRolesToggle;

    [Header("All roles visuals")]
    [SerializeField] private GameObject _allRolesChecked;
    [SerializeField] private GameObject _allRolesUnchecked;

    [Header("Buttonss")]
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private GameObject _filtersPopup;  

    [SerializeField] private ClanRoleCatalog _roleCatalog;
    [SerializeField] private Sprite _fallbackIcon;

    public Action<MemberListFilters> OnFiltersChanged;

    public event Action Closed;

    private MemberSort _memberSort = MemberSort.MostWins;
    private MemberSort _appliedMemberSort = MemberSort.MostWins;

    private MemberSort[] _sortValues;
    private int _sortIndex;

    private HashSet<string> _selectedRoles = new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _appliedRoles = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ClanRoleItemTemplate> _roleItems = new();


    private void Awake()
    {
        InitializeEnumArrays();
    }

    private void OnEnable()
    {
        LoadDraftFromApplied();
        InitSelectors();
        SetupButtons();
    }

    public void OpenForClan(string clanId)
    {
        if (_filtersPopup != null)
            _filtersPopup.SetActive(true);
        else
            gameObject.SetActive(true);

        if (isActiveAndEnabled)
            StartCoroutine(FetchRolesFromServer(clanId));
    }

    private IEnumerator FetchRolesFromServer(string clanId)
    {
        if (string.IsNullOrEmpty(clanId))
            clanId = ServerManager.Instance?.Clan?._id;

        if (string.IsNullOrEmpty(clanId))
            yield break;

        ClanData clanData = null;
        Storefront.Get().GetClanData(clanId, data => clanData = data);

        yield return new WaitUntil(() => clanData != null);

        var roles = clanData.ClanRoles?
            .Where(r => r != null && !string.IsNullOrEmpty(r.name))
            .GroupBy(r => r.name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList()
            ?? new List<ClanRoles>();

        SetAvailableRoles(roles, _selectedRoles.ToList());
    }

    private void InitializeAllRolesToggle()
    {
        if (_allRolesToggle == null) return;

        _allRolesToggle.onValueChanged.RemoveAllListeners();

        bool showAllRoles = _selectedRoles.Count == 0;
        SetAllRolesVisuals(showAllRoles);

        _allRolesToggle.onValueChanged.AddListener(isOn =>
        {
            if (!isOn)
            {
                SetAllRolesVisuals(false);
                return;
            }

            _selectedRoles.Clear();
            RefreshRoleToggleVisuals();
            SetAllRolesVisuals(true);
        });
    }

    private void RefreshRoleToggleVisuals()
    {
        foreach (var item in _roleItems)
        {
            if (item == null) continue;

            item.SetSelectedWithoutNotify(false);
        }
    }

    private void InitializeEnumArrays()
    {
        _sortValues = GetEnumValues<MemberSort>();
        _sortIndex = FindEnumIndex(_sortValues, _memberSort);
    }

    private T[] GetEnumValues<T>() where T : Enum
    {
        return (T[])Enum.GetValues(typeof(T));
    }

    private int FindEnumIndex<T>(T[] values, T targetValue) where T : Enum
    {
        if (values == null || values.Length == 0)
            return 0;

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].Equals(targetValue))
                return i;
        }

        return 0;
    }

    private void SetAllRolesVisuals(bool isOn)
    {
        if (_allRolesToggle != null)
            _allRolesToggle.SetIsOnWithoutNotify(isOn);

        if (_allRolesChecked != null)
            _allRolesChecked.SetActive(isOn);

        if (_allRolesUnchecked != null)
            _allRolesUnchecked.SetActive(!isOn);
    }

    private void SetupButtons()
    {
        if (_confirmButton != null) _confirmButton.onClick.RemoveAllListeners();
        if (_cancelButton != null) _cancelButton.onClick.RemoveAllListeners();
        if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();

        if (_confirmButton != null)
            _confirmButton.onClick.AddListener(() =>
            {
                CommitDraftToApplied();
                UpdateFilters();
                Close();
            });

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(CloseWithoutApplying);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(CloseWithoutApplying);
    }

    public void CloseWithoutApplying()
    {
        LoadDraftFromApplied();
        Close();
    }

    private void Close()
    {
        if (_filtersPopup != null)
            _filtersPopup.SetActive(false);
        else
            gameObject.SetActive(false);

        Closed?.Invoke();
    }

    private void InitSelectors()
    {
        InitializeSortDropdown();
        InitializeAllRolesToggle();
    }

    private void InitializeSortDropdown()
    {
        if (_sortDropdown == null) return;

        _sortDropdown.onValueChanged.RemoveAllListeners();
        _sortDropdown.ClearOptions();

        List<string> options = _sortValues
            .Select(GetMemberSortText)
            .ToList();

        _sortDropdown.AddOptions(options);

        _sortIndex = FindEnumIndex(_sortValues, _memberSort);

        _sortDropdown.SetValueWithoutNotify(_sortIndex);
        _sortDropdown.RefreshShownValue();

        _sortDropdown.onValueChanged.AddListener(index =>
        {
            if (index < 0 || index >= _sortValues.Length) return;

            _sortIndex = index;
            _memberSort = _sortValues[_sortIndex];
        });
    }

    private string GetMemberSortText(MemberSort sort)
    {
        return sort switch
        {
            MemberSort.MostWins => "Eniten voittoja",
            MemberSort.OldestMemberFirst => "Vanhin jäsen ensin",
            MemberSort.NewestMemberFirst => "Uusin jäsen ensin",
            MemberSort.LeastWins => "Vähiten voittoja",
            MemberSort.NameAscending => "A - Ö",
            MemberSort.NameDescending => "Ö - A",
            _ => "Eniten voittoja"
        };
    }

    private void UpdateFilters()
    {
        OnFiltersChanged?.Invoke(new MemberListFilters
        {
            memberSort = _memberSort,
            selectedRoles = _selectedRoles.ToList()
        });
    }

    public void SetAvailableRoles(List<ClanRoles> roles, IEnumerable<string> preselected = null)
    {
        if (_rolesContent == null || _roleItemPrefab == null) return;

        _roleItems.Clear();

        for (int i = _rolesContent.childCount - 1; i >= 0; i--)
            Destroy(_rolesContent.GetChild(i).gameObject);

        _selectedRoles.Clear();

        if (preselected != null)
        {
            foreach (var r in preselected)
            {
                if (!string.IsNullOrWhiteSpace(r))
                    _selectedRoles.Add(r);
            }
        }

        if (roles == null) return;

        foreach (var role in roles
                     .Where(r => r != null && !string.IsNullOrEmpty(r.name))
                     .GroupBy(r => r.name, StringComparer.OrdinalIgnoreCase)
                     .Select(g => g.First()))
        {
            string rawName = role.name;
            string displayName = _roleCatalog != null ? _roleCatalog.GetDisplayName(rawName) : rawName;
            var icon = _roleCatalog != null ? _roleCatalog.GetIcon(rawName) : null;

            if (icon == null)
                icon = _fallbackIcon;

            var item = Instantiate(_roleItemPrefab, _rolesContent);
            item.gameObject.SetActive(true);

            bool isOn = _selectedRoles.Contains(rawName);
            item.Init(rawName, displayName, icon, isOn, OnRoleToggled);

            _roleItems.Add(item);
        }

        InitializeAllRolesToggle();
    }

    private void OnRoleToggled(string roleName, bool isOn)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return;

        if (isOn)
        {
            _selectedRoles.Add(roleName);
            SetAllRolesVisuals(false);
        }
        else
        {
            _selectedRoles.Remove(roleName);

            if (_selectedRoles.Count == 0)
                SetAllRolesVisuals(true);
        }
    }

    private void LoadDraftFromApplied()
    {
        if (_sortValues == null || _sortValues.Length == 0)
            InitializeEnumArrays();

        _memberSort = _appliedMemberSort;

        _selectedRoles.Clear();

        foreach (var role in _appliedRoles)
        {
            _selectedRoles.Add(role);
        }

        _sortIndex = FindEnumIndex(_sortValues, _memberSort);

        if (_sortDropdown != null)
        {
            _sortDropdown.SetValueWithoutNotify(_sortIndex);
            _sortDropdown.RefreshShownValue();
        }
    }

    private void CommitDraftToApplied()
    {
        _appliedMemberSort = _memberSort;

        _appliedRoles.Clear();
        foreach (var r in _selectedRoles)
            _appliedRoles.Add(r);
    }
}

