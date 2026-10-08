using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts;
using UnityEngine;

public class ClanMemberRoleRights : MonoBehaviour
{
    public ClanMember CurrentMember { get; set; }

    private ClanData _clanData = null;

    public void MemberRoleRights()
    {
        Debug.Log("Hello there!");

        // !-- Remember to delete, test for seeing role rights

        Storefront.Get().GetClanData(ServerManager.Instance.Player.clan_id, data =>
        {
            _clanData = data;
        });

        foreach (var role in _clanData.ClanRoles)
        {
            if (role.rights != null)
                PopulateRolesAndRights(role);
        }
        // Ends here

        return;
    }

    private void PopulateRolesAndRights(ClanRoles currentRole)
    {
        foreach (PlayerRights right in Enum.GetValues(typeof(PlayerRights)))
        {
            bool isAllowed = currentRole.HasRights(right);
            Debug.Log($"{right} {isAllowed}");
        }
    }
}
