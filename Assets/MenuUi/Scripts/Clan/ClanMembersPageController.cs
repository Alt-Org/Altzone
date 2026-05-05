using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;

public class ClanMembersPageController : MonoBehaviour
{
    [SerializeField] private Transform _membersContent;
    [SerializeField] private ClanMemberPlaque _memberPlaquePrefab;
    [SerializeField] private ClanMemberPopupController _memberPopup;
    [SerializeField] private TMP_InputField _memberSearchInput;
    [SerializeField] private ClanAddFriendPopupController _addFriendPopup;

    [SerializeField] private ClanRoleSelectPopupController _roleSelectPopup;
    [SerializeField] private Canvas _canvas;

    [SerializeField] private ClanMainView _clanMainView;

    private string _memberSearchText = string.Empty;
    private string _viewedClanId;
    private HashSet<string> _viewAdminSet;

    private Coroutine _rebuildRoutine;

    private HashSet<string> _roleFilterSet;

    private ClanMembersFiltersPopup.MemberSort _memberSort = ClanMembersFiltersPopup.MemberSort.MostWins;

    private List<ClanMember> _cachedMembers;
    private string _cachedClanId;

    private void OnEnable()
    {
        _memberSearchText = string.Empty;

        if (_memberSearchInput != null)
        {
            _memberSearchInput.SetTextWithoutNotify(string.Empty);
        }

        if (_memberSearchInput != null)
        {
            _memberSearchInput.onValueChanged.RemoveListener(OnMemberSearchChanged);
            _memberSearchInput.onValueChanged.AddListener(OnMemberSearchChanged);
        }

        if (!string.IsNullOrEmpty(_viewedClanId))
        {
            Rebuild();
        }
    }

    private void OnDisable()
    {
        if (_memberSearchInput != null)
        {
            _memberSearchInput.onValueChanged.RemoveListener(OnMemberSearchChanged);
        }
    }

    private void Rebuild(bool forceFetch = false)
    {
        if (!isActiveAndEnabled) return;

        if (_rebuildRoutine != null)
        {
            StopCoroutine(_rebuildRoutine);
        }

        _rebuildRoutine = StartCoroutine(RebuildCoroutine(forceFetch));
    }

    private IEnumerator RebuildCoroutine(bool forceFetch)
    {
        for (int i = _membersContent.childCount - 1; i >= 0; i--)
            Destroy(_membersContent.GetChild(i).gameObject);

        // Fetch members only if needed
        List<ClanMember> members = null;

        bool needFetch =
            forceFetch ||
            _cachedMembers == null ||
            _cachedClanId != _viewedClanId;

        if (needFetch)
        {
            if (!string.IsNullOrEmpty(_viewedClanId))
            {
                yield return StartCoroutine(ServerManager.Instance.GetClanMembersFromServer(_viewedClanId, m => members = m));
            }
            else
            {
                yield return StartCoroutine(ServerManager.Instance.GetClanPlayers(m => members = m));
            }

            if (members == null) yield break;

            _cachedMembers = members;
            _cachedClanId = _viewedClanId;
        }
        else
        {
            members = _cachedMembers;
        }

        var clan = ServerManager.Instance.Clan;

        // Admin set as a fallback label if member.Role is missing
        var adminSet = _viewAdminSet
            ?? new HashSet<string>();

        // Build sortable rows
        var rowsQuery = members
            .Where(m => m != null)
            .Select(m =>
            {
                int rightsCount = CountRights(m.Role);
                string roleName = GetRoleLabel(m, adminSet);
                string playerName = m.Name ?? string.Empty;
                int wins = GetMemberWins(m);
                DateTime joinedAt = GetMemberJoinedAt(m);

                return new MemberRow(m, rightsCount, roleName, playerName, wins, joinedAt);
            });

        // Role filter: if user chooses roles, show only those roles in the list ---
        if (_roleFilterSet != null && _roleFilterSet.Count > 0)
        {
            rowsQuery = rowsQuery.Where(r => _roleFilterSet.Contains(r.RoleLabel));
        }

        if (!string.IsNullOrWhiteSpace(_memberSearchText))
        {
            string search = _memberSearchText.Trim();

            rowsQuery = rowsQuery.Where(r =>
                !string.IsNullOrEmpty(r.PlayerName) &&
                r.PlayerName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        List<MemberRow> rows;
        switch (_memberSort)
        {
            case ClanMembersFiltersPopup.MemberSort.MostWins:
                rows = rowsQuery
                    .OrderByDescending(r => r.Wins)
                    .ThenBy(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;

            case ClanMembersFiltersPopup.MemberSort.LeastWins:
                rows = rowsQuery
                    .OrderBy(r => r.Wins)
                    .ThenBy(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;

            case ClanMembersFiltersPopup.MemberSort.OldestMemberFirst:
                rows = rowsQuery
                    .OrderBy(r => r.JoinedAt)
                    .ThenBy(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;

            case ClanMembersFiltersPopup.MemberSort.NewestMemberFirst:
                rows = rowsQuery
                    .OrderByDescending(r => r.JoinedAt)
                    .ThenBy(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;

            case ClanMembersFiltersPopup.MemberSort.NameAscending:
                rows = rowsQuery
                    .OrderBy(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;

            case ClanMembersFiltersPopup.MemberSort.NameDescending:
                rows = rowsQuery
                    .OrderByDescending(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;

            default:
                rows = rowsQuery
                    .OrderByDescending(r => r.Wins)
                    .ThenBy(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var member = row.Member;

            var plaque = Instantiate(_memberPlaquePrefab, _membersContent);

            bool isOwnClan = string.IsNullOrEmpty(_viewedClanId)
             || (ServerManager.Instance.Clan != null && _viewedClanId == ServerManager.Instance.Clan._id);

            bool isCurrentPlayer = IsCurrentPlayerMember(member);

            plaque.SetAddFriendButtonVisible(!isCurrentPlayer);

            plaque.SetVoteInteractable(isOwnClan);

            plaque.BindVote(() =>
            {
                if (!isOwnClan) return;

                var roles = ServerManager.Instance?.Clan?.roles;
                if (roles == null || _roleSelectPopup == null || _canvas == null) return;

                // anchor near the vote button (preferred), fallback to plaque rect
                var anchor = plaque.VoteButtonRect != null ? plaque.VoteButtonRect : plaque.GetComponent<RectTransform>();
                _roleSelectPopup.ShowAnchored(member, roles, anchor, _canvas);
            });

            plaque.BindAddFriend(() =>
            {
                if (!isCurrentPlayer)
                {
                    plaque.BindAddFriend(() =>
                    {
                        if (_clanMainView == null) return;

                        _clanMainView.OpenAddFriendPopup(member);
                    });
                }
            });

            plaque.gameObject.SetActive(true);

            plaque.SetPosition(i + 1);
            plaque.SetName(member?.Name);
            plaque.SetRole(row.RoleLabel);
            plaque.SetActivityRosette(false);

            // Avatar
            var faceLoader = plaque.gameObject.GetComponentInChildren<MenuUi.Scripts.AvatarEditor.AvatarFaceLoader>(true);
            if (faceLoader != null)
            {
                faceLoader.SetUseOwnAvatarVisuals(false);

                var avatarData = member.AvatarData;
                if (avatarData != null && AvatarDesignLoader.Instance != null)
                {
                    var visualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(avatarData);
                    if (visualData != null)
                        faceLoader.UpdateVisuals(visualData);
                }
            }

            // Popup button
            var button = plaque.GetComponent<Button>();
            if (button != null && _memberPopup != null)
            {
                var capturedMember = member;
                var capturedRoleLabel = row.RoleLabel;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    _memberPopup.Show(capturedMember, capturedRoleLabel, allowVotes: isOwnClan);
                });
            }
        }

        _rebuildRoutine = null;
    }

    private void OnMemberSearchChanged(string searchText)
    {
        _memberSearchText = searchText ?? string.Empty;
        Rebuild(forceFetch: false);
    }

    private static int GetMemberWins(ClanMember member)
    {
        if (member == null) return 0;

        return member.LeaderBoardWins;
    }

    private static DateTime GetMemberJoinedAt(ClanMember member)
    {
        return DateTime.MaxValue;
    }

    public void SetMemberSort(ClanMembersFiltersPopup.MemberSort sort)
    {
        _memberSort = sort;
        Rebuild(forceFetch: false);
    }

    // Shows members of the specified clan.
    public void SetViewedClan(string clanId, IEnumerable<string> adminIds = null)
    {
        _viewedClanId = clanId;
        _viewAdminSet = adminIds != null ? new HashSet<string>(adminIds) : null;

        Rebuild(forceFetch: true);
    }

    public void ApplyFilters(ClanMembersFiltersPopup.MemberSort sort, List<string> selectedRoles)
    {
        _memberSort = sort;

        if (selectedRoles != null && selectedRoles.Count > 0)
            _roleFilterSet = new HashSet<string>(selectedRoles, StringComparer.OrdinalIgnoreCase);
        else
            _roleFilterSet = null;

        Rebuild(forceFetch: false);
    }

    private static int CountRights(ClanRoles role)
    {
        if (role == null || role.rights == null) return 0;

        int count = 0;
        var r = role.rights;

        if (r.edit_soulhome) count++;
        if (r.edit_clan_data) count++;
        if (r.edit_member_rights) count++;
        if (r.manage_role) count++;
        if (r.shop) count++;

        return count;
    }

    private static string GetRoleLabel(ClanMember member, HashSet<string> adminSet)
    {
        if (member?.Role != null && !string.IsNullOrEmpty(member.Role.name))
            return member.Role.name;

        if (member != null && adminSet != null && adminSet.Contains(member.Id))
            return "Admin";

        return "Member";
    }

    private readonly struct MemberRow
    {
        public ClanMember Member { get; }
        public int RightsCount { get; }
        public string RoleLabel { get; }
        public string PlayerName { get; }

        public int Wins { get; }
        public DateTime JoinedAt { get; }

        public MemberRow(
            ClanMember member,
            int rightsCount,
            string roleLabel,
            string playerName,
            int wins,
            DateTime joinedAt)
        {
            Member = member;
            RightsCount = rightsCount;
            RoleLabel = roleLabel ?? string.Empty;
            PlayerName = playerName ?? string.Empty;
            Wins = wins;
            JoinedAt = joinedAt;
        }
    }

    private static bool IsCurrentPlayerMember(ClanMember member)
    {
        if (member == null || ServerManager.Instance == null || ServerManager.Instance.Player == null)
            return false;

        var currentPlayer = ServerManager.Instance.Player;

        if (!string.IsNullOrEmpty(member.Id) && !string.IsNullOrEmpty(currentPlayer._id))
            return member.Id == currentPlayer._id;

        if (!string.IsNullOrEmpty(member.Name) && !string.IsNullOrEmpty(currentPlayer.name))
            return string.Equals(member.Name, currentPlayer.name, StringComparison.OrdinalIgnoreCase);

        return false;
    }
}

