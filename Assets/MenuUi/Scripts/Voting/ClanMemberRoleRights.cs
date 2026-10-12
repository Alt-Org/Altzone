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
        Debug.Log($"We got CurrentMember: {CurrentMember.Player.name}");

        if (CurrentMember.Role.rights != null)
            PopulateRolesAndRights(CurrentMember.Role);

        return;
    }

    private void PopulateRolesAndRights(ClanRoles currentRole)
    {
        foreach (PlayerRights right in Enum.GetValues(typeof(PlayerRights)))
        {
            bool isAllowed = currentRole.HasRights(right);

            // currentRole.SetRights(right, true);

            Debug.Log($"{right} {currentRole.HasRights(right)}");
        }
    }
}
