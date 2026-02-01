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
    public enum NameSort
    {
        None,
        Asc,   // A–Ö
        Desc   // Ö–A
    }

    [Serializable]
    public struct MemberListFilters
    {
        public NameSort nameSort;
        public ClanRanking ranking;     
        public ClanActivity activity;   
        public List<string> selectedRoles;
    }

    [Header("Name Selection")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Button _nameButtonPrevious;
    [SerializeField] private Button _nameButtonNext;

    [Header("Ranking Selection")]
    [SerializeField] private TextMeshProUGUI _rankingText;
    [SerializeField] private Button _rankingButtonPrevious;
    [SerializeField] private Button _rankingButtonNext;

    [Header("Activity Selection")]
    [SerializeField] private TextMeshProUGUI _activityText;
    [SerializeField] private Button _activityButtonPrevious;
    [SerializeField] private Button _activityButtonNext;

    [Header("Roles selection (multi-select list)")]
    [SerializeField] private Transform _rolesContent;
    [SerializeField] private ClanRoleItemTemplate _roleItemPrefab;

    [Header("Buttons")]
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;     
    [SerializeField] private GameObject _filtersPopup;  

    [SerializeField] private ClanRoleCatalog _roleCatalog;
    [SerializeField] private Sprite _fallbackIcon;

    public Action<MemberListFilters> OnFiltersChanged;

    public event Action Closed;

    // Current selections
    private NameSort _nameSort = NameSort.None;
    private ClanRanking _clanRanking = ClanRanking.None;
    private ClanActivity _clanActivity = ClanActivity.None;
    private HashSet<string> _selectedRoles = new(StringComparer.OrdinalIgnoreCase);

    // Applied (last confirmed) selections
    private NameSort _appliedNameSort = NameSort.None;
    private ClanRanking _appliedRanking = ClanRanking.None;
    private ClanActivity _appliedActivity = ClanActivity.None;
    private readonly HashSet<string> _appliedRoles = new(StringComparer.OrdinalIgnoreCase);


    private NameSort[] _nameValues;
    private ClanRanking[] _rankingValues;
    private ClanActivity[] _activityValues;

    private int _nameIndex;
    private int _rankingIndex;
    private int _activityIndex;

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

    private void OnDisable()
    {
        // optional: hide popup root
        if (_filtersPopup != null) _filtersPopup.SetActive(false);
    }

    public void OpenForClan(string clanId)
    {
        if (_filtersPopup != null) _filtersPopup.SetActive(true);
        else gameObject.SetActive(true);

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

    private void InitializeEnumArrays()
    {
        _nameValues = GetEnumValues<NameSort>();
        _rankingValues = GetEnumValues<ClanRanking>();
        _activityValues = GetEnumValues<ClanActivity>();

        _nameIndex = FindEnumIndex(_nameValues, _nameSort);
        _rankingIndex = FindEnumIndex(_rankingValues, _clanRanking);
        _activityIndex = FindEnumIndex(_activityValues, _clanActivity);
    }

    private T[] GetEnumValues<T>() where T : Enum
    {
        return (T[])Enum.GetValues(typeof(T));
    }

    private int FindEnumIndex<T>(T[] values, T targetValue) where T : Enum
    {
        for (int i = 0; i < values.Length; i++)
            if (values[i].Equals(targetValue))
                return i;
        return 0;
    }

    private void SetupButtons()
    {
        if (_confirmButton != null) _confirmButton.onClick.RemoveAllListeners();
        if (_cancelButton != null) _cancelButton.onClick.RemoveAllListeners();

        if (_confirmButton != null)
            _confirmButton.onClick.AddListener(() =>
            {
                UpdateFilters();
                if (_filtersPopup != null) _filtersPopup.SetActive(false);
                Closed?.Invoke();
            });

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(() =>
            {
                if (_filtersPopup != null) _filtersPopup.SetActive(false);
                Closed?.Invoke();
            });
    }

    private void InitSelectors()
    {
        InitializeName();
        InitializeRanking();
        InitializeActivity();
    }

    private void InitializeName()
    {
        if (_nameText != null)
            _nameText.text = GetNameSortText(_nameSort);

        if (_nameButtonNext != null) _nameButtonNext.onClick.RemoveAllListeners();
        if (_nameButtonPrevious != null) _nameButtonPrevious.onClick.RemoveAllListeners();

        if (_nameButtonNext != null)
        {
            _nameButtonNext.onClick.AddListener(() =>
            {
                _nameIndex = GetNextIndex(_nameIndex, _nameValues.Length);
                _nameSort = _nameValues[_nameIndex];
                if (_nameText != null) _nameText.text = GetNameSortText(_nameSort);
            });
        }

        if (_nameButtonPrevious != null)
        {
            _nameButtonPrevious.onClick.AddListener(() =>
            {
                _nameIndex = GetPreviousIndex(_nameIndex, _nameValues.Length);
                _nameSort = _nameValues[_nameIndex];
                if (_nameText != null) _nameText.text = GetNameSortText(_nameSort);
            });
        }
    }

    private void InitializeRanking()
    {
        if (_rankingText != null)
            _rankingText.text = ClanDataTypeConverter.GetRankingText(_clanRanking);

        if (_rankingButtonNext != null) _rankingButtonNext.onClick.RemoveAllListeners();
        if (_rankingButtonPrevious != null) _rankingButtonPrevious.onClick.RemoveAllListeners();

        if (_rankingButtonNext != null)
        {
            _rankingButtonNext.onClick.AddListener(() =>
            {
                _rankingIndex = GetNextIndex(_rankingIndex, _rankingValues.Length);
                _clanRanking = _rankingValues[_rankingIndex];
                if (_rankingText != null) _rankingText.text = ClanDataTypeConverter.GetRankingText(_clanRanking);
            });
        }

        if (_rankingButtonPrevious != null)
        {
            _rankingButtonPrevious.onClick.AddListener(() =>
            {
                _rankingIndex = GetPreviousIndex(_rankingIndex, _rankingValues.Length);
                _clanRanking = _rankingValues[_rankingIndex];
                if (_rankingText != null) _rankingText.text = ClanDataTypeConverter.GetRankingText(_clanRanking);
            });
        }
    }

    private void InitializeActivity()
    {
        if (_activityText != null)
            _activityText.text = ClanDataTypeConverter.GetActivityText(_clanActivity);

        if (_activityButtonNext != null) _activityButtonNext.onClick.RemoveAllListeners();
        if (_activityButtonPrevious != null) _activityButtonPrevious.onClick.RemoveAllListeners();

        if (_activityButtonNext != null)
        {
            _activityButtonNext.onClick.AddListener(() =>
            {
                _activityIndex = GetNextIndex(_activityIndex, _activityValues.Length);
                _clanActivity = _activityValues[_activityIndex];
                if (_activityText != null) _activityText.text = ClanDataTypeConverter.GetActivityText(_clanActivity);
            });
        }

        if (_activityButtonPrevious != null)
        {
            _activityButtonPrevious.onClick.AddListener(() =>
            {
                _activityIndex = GetPreviousIndex(_activityIndex, _activityValues.Length);
                _clanActivity = _activityValues[_activityIndex];
                if (_activityText != null) _activityText.text = ClanDataTypeConverter.GetActivityText(_clanActivity);
            });
        }
    }

    private int GetNextIndex(int currentIndex, int arrayLength)
    {
        return (currentIndex + 1) % arrayLength;
    }

    private int GetPreviousIndex(int currentIndex, int arrayLength)
    {
        return currentIndex == 0 ? arrayLength - 1 : currentIndex - 1;
    }

    private string GetNameSortText(NameSort sort)
    {
        return sort switch
        {
            NameSort.None => "Oletus",
            NameSort.Asc => "A\u2013\u00D6", // A–Ö
            NameSort.Desc => "\u00D6\u2013A", // Ö–A
            _ => "Oletus"
        };
    }

    private void UpdateFilters()
    {
        OnFiltersChanged?.Invoke(new MemberListFilters
        {
            nameSort = _nameSort,
            ranking = _clanRanking,
            activity = _clanActivity,
            selectedRoles = _selectedRoles.ToList()
        });
    }

    public void SetAvailableRoles(List<ClanRoles> roles, IEnumerable<string> preselected = null)
    {
        if (_rolesContent == null || _roleItemPrefab == null) return;

        for (int i = _rolesContent.childCount - 1; i >= 0; i--)
            Destroy(_rolesContent.GetChild(i).gameObject);

        _selectedRoles.Clear();
        if (preselected != null)
        {
            foreach (var r in preselected)
                if (!string.IsNullOrWhiteSpace(r))
                    _selectedRoles.Add(r);
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
            if (icon == null) icon = _fallbackIcon;

            var item = Instantiate(_roleItemPrefab, _rolesContent);
            item.gameObject.SetActive(true);

            bool isOn = _selectedRoles.Contains(rawName);
            item.Init(rawName, displayName, icon, isOn, OnRoleToggled);
        }
    }

    private void OnRoleToggled(string roleName, bool isOn)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return;

        if (isOn) _selectedRoles.Add(roleName);
        else _selectedRoles.Remove(roleName);
    }

    private void LoadDraftFromApplied()
    {
        _nameSort = _appliedNameSort;
        _clanRanking = _appliedRanking;
        _clanActivity = _appliedActivity;

        _selectedRoles.Clear();
        foreach (var r in _appliedRoles)
            _selectedRoles.Add(r);

        _nameIndex = FindEnumIndex(_nameValues, _nameSort);
        _rankingIndex = FindEnumIndex(_rankingValues, _clanRanking);
        _activityIndex = FindEnumIndex(_activityValues, _clanActivity);
    }

    private void CommitDraftToApplied()
    {
        _appliedNameSort = _nameSort;
        _appliedRanking = _clanRanking;
        _appliedActivity = _clanActivity;

        _appliedRoles.Clear();
        foreach (var r in _selectedRoles)
            _appliedRoles.Add(r);
    }
}

