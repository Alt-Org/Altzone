using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class ClanRightsDebugger : MonoBehaviour
{
    public void OnDebugButtonPressed()
    {
        StartCoroutine(DebugClanRightsCoroutine());
    }

    private IEnumerator DebugClanRightsCoroutine()
    {
        // Get the player's unique ID 
        string displayId = GameConfig.Get().PlayerSettings.PlayerGuid;

        PlayerData playerData = null;
        ClanData clanData = null;
        float maxWaitTime = 5f; 
        float timer = 0f;

        // Fetch player data based on their ID
        Storefront.Get().GetPlayerData(displayId, data => playerData = data);

        // Timeout if playerdata is not retrieved fast enough
        while (playerData == null && timer < maxWaitTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (playerData == null)
        {
            Debug.LogWarning("Player data not found, or timer ran out.");
            yield break;
        }

        string playerId = playerData.Id;

        if (string.IsNullOrEmpty(playerData.ClanId))
        {
            Debug.LogWarning("Player is not in a clan.");
            yield break;
        }

        // Fetch clan data based on the player's clan ID
        Storefront.Get().GetClanData(playerData.ClanId, data => clanData = data);

        // Reset the timer
        timer = 0f;

        // Timeout if clandata is not retrieved fast enough
        while (clanData == null && timer < maxWaitTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (clanData == null)
        {
            Debug.LogWarning("Clan data not found or timed out.");
            yield break;
        }

        // Log the clan member count
        Debug.Log($"Clan members count: {clanData.Members.Count}");

        // Log every player's ID, Name and Role
        foreach (var member in clanData.Members)
        {
            Debug.Log($"ClanMember ID: '{member.Id}', Name: {member.Name}, Role: {member.Role}");
        }

        // Find the current player in the clan members list
        ClanMember clanMember = clanData.Members.Find(m =>
            !string.IsNullOrEmpty(m.Id) &&
            string.Equals(m.Id.Trim(), playerId.Trim(), System.StringComparison.OrdinalIgnoreCase));

        if (clanMember == null)
        {
            Debug.LogWarning("Player not found in clan members.");
            yield break;
        }

        // Log the player's role
        Debug.Log($"clanMember.Role string value: {clanMember.Role.name}");

        // Convert string to enum
        /*if (!Enum.TryParse(clanMember.Role, out ClanMemberRole parsedRole))
        /{
            Debug.LogWarning($"Invalid role value: {clanMember.Role}");
            yield break;
        }

        // Get the index for this role
        int roleIndex = GetRoleIndex(parsedRole);

        // Check if role index is valid
        if (roleIndex < 0 || roleIndex >= clanData.ClanRights.Length)
        {
            Debug.LogWarning("Invalid role index.");
            yield break;
        }

        // Log the rights that come with the role
        ClanRoleRights rights = clanData.ClanRights[roleIndex];
        Debug.Log($"Player Role: {clanMember.Role}, Rights: {rights}");*/
    }

    private int GetRoleIndex(ClanMemberRole role)
    {
        switch (role)
        {
            case ClanMemberRole.None:
            case ClanMemberRole.Member: return 0;
            case ClanMemberRole.Officer: return 1;
            case ClanMemberRole.Admin: return 2;
            default: return -1;
        }
    }
}
