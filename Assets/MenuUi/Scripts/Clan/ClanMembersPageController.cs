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

    private void OnEnable()
    {
        if(!string.IsNullOrEmpty(_viewedClanId))
        {
            Rebuild();
        }     
    }

    private void Rebuild()
    {
        if (!isActiveAndEnabled) return;

        if (_rebuildRoutine != null)
        {
            StopCoroutine(_rebuildRoutine);
        }

        _rebuildRoutine = StartCoroutine(RebuildCoroutine());
    }

    private IEnumerator RebuildCoroutine()
    {
        for (int i = _membersContent.childCount - 1; i >= 0; i--)
            Destroy(_membersContent.GetChild(i).gameObject);

        // Fetch members
        List<ClanMember> members = null;
        if (!string.IsNullOrEmpty(_viewedClanId))
        {
            yield return StartCoroutine(ServerManager.Instance.GetClanMembersFromServer(_viewedClanId, m => members = m));
        }
        else
        {
            yield return StartCoroutine(ServerManager.Instance.GetClanPlayers(m => members = m));
        }

        if (members == null) yield break;

        var clan = ServerManager.Instance.Clan;

        // Admin set as a fallback label if member.Role is missing
        var adminSet = _viewAdminSet
            ?? new HashSet<string>(); 

        // Build sortable rows
        var rows = members
            .Where(m => m != null)
            .Select(m =>
            {
                int rightsCount = CountRights(m.Role);
                string roleName = GetRoleLabel(m, adminSet); // used for display + tie-breaking
                string playerName = m.Name ?? string.Empty;

                return new MemberRow(m, rightsCount, roleName, playerName);
            })
            // Most rights on top
            .OrderByDescending(r => r.RightsCount)
            // Same rights -> alphabetical: first role, then player name
            .ThenBy(r => r.RoleLabel, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.PlayerName, StringComparer.OrdinalIgnoreCase)
            .ToList();

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
                    _memberPopup.Show(capturedMember, capturedRoleLabel);
                });
            }
        }

        _rebuildRoutine = null;
    }

    public void SetViewedClan(string clanId, IEnumerable<string> adminIds = null)
    {
        _viewedClanId = clanId;
        _viewAdminSet = adminIds != null ? new HashSet<string>(adminIds) : null;

        Rebuild();
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

