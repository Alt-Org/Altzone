using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;

public class ClanMembersPageController : MonoBehaviour
{
    [SerializeField] private Transform _membersContent;
    [SerializeField] private ClanMemberPlaque _memberPlaquePrefab;
    [SerializeField] private ClanMemberPopupController _memberPopup;

    private string _viewedClanId;
    private HashSet<string> _viewAdminSet;

    private Coroutine _rebuildRoutine;

    private HashSet<string> _roleFilterSet;

    private ClanMembersFiltersPopup.NameSort _nameSort = ClanMembersFiltersPopup.NameSort.None;

    private List<ClanMember> _cachedMembers;
    private string _cachedClanId;

    private void OnEnable()
    {
        if(!string.IsNullOrEmpty(_viewedClanId))
        {
            Rebuild();
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
                return new MemberRow(m, rightsCount, roleName, playerName);
            });

        // Role filter: if user chooses roles, show only those roles in the list ---
        if (_roleFilterSet != null && _roleFilterSet.Count > 0)
        {
            rowsQuery = rowsQuery.Where(r => _roleFilterSet.Contains(r.RoleLabel));
        }

        List<MemberRow> rows;
        switch (_nameSort)
        {
            case ClanMembersFiltersPopup.NameSort.Asc:
                rows = rowsQuery
                    .OrderBy(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ThenByDescending(r => r.RightsCount)
                    .ThenBy(r => r.RoleLabel, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;

            case ClanMembersFiltersPopup.NameSort.Desc:
                rows = rowsQuery
                    .OrderByDescending(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ThenByDescending(r => r.RightsCount)
                    .ThenBy(r => r.RoleLabel, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;

            default: // Default = member list sorted by role 
                rows = rowsQuery
                    .OrderByDescending(r => r.RightsCount)
                    .ThenBy(r => r.RoleLabel, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                break;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var member = row.Member;

            var plaque = Instantiate(_memberPlaquePrefab, _membersContent);
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

                    // IMPORTANT: member list uses RectMask2D clipping.
                    // Avatar visuals may assign a custom material that ignores UI clipping,
                    // so we force default UI material (null) for all avatar Images under this plaque.
                    ClearUIMaterials(faceLoader.transform);
                }
            }

            // Popup button
            var button = plaque.GetComponent<Button>();
            if (button != null && _memberPopup != null)
            {
                var capturedMember = member;
                var capturedRoleLabel = row.RoleLabel;

                bool isOwnClan = string.IsNullOrEmpty(_viewedClanId)
                || (ServerManager.Instance.Clan != null && _viewedClanId == ServerManager.Instance.Clan._id);


                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    _memberPopup.Show(capturedMember, capturedRoleLabel, allowVotes: isOwnClan);
                });
            }
        }

        _rebuildRoutine = null;
    }

    private static void ClearUIMaterials(Transform root)
    {
        if (root == null) return;

        var images = root.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img == null) continue;

            // Forces Unity UI default material/shader (supports RectMask2D clipping)
            img.material = null;

            // Optional safety: ensure it can be clipped by masks
            img.maskable = true;
        }
    }


    public void SetNameSort(ClanMembersFiltersPopup.NameSort sort)
    {
        _nameSort = sort;
        Rebuild(forceFetch: false);
    }

    // Shows members of the specified clan.
    public void SetViewedClan(string clanId, IEnumerable<string> adminIds = null)
    {
        _viewedClanId = clanId;
        _viewAdminSet = adminIds != null ? new HashSet<string>(adminIds) : null;

        Rebuild(forceFetch: true);
    }

    public void ApplyFilters(ClanMembersFiltersPopup.NameSort sort, List<string> selectedRoles)
    {
        _nameSort = sort;

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

        public MemberRow(ClanMember member, int rightsCount, string roleLabel, string playerName)
        {
            Member = member;
            RightsCount = rightsCount;
            RoleLabel = roleLabel ?? string.Empty;
            PlayerName = playerName ?? string.Empty;
        }
    }
}

