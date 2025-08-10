using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Voting;
using UnityEngine;

public static class ClanPollManager
{
    // Creates Role Poll
    public static void CreateRolePromotionPoll(string targetPlayerId, ClanMemberRole targetRole)
    {
        var store = Storefront.Get();
        PlayerData player = null;
        ClanData clan = null;

        // Retrieve data for the current player 
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);
        store.GetClanData(player.ClanId, data => clan = data);

        if (clan == null || player == null)
        {
            Debug.LogWarning("Clan or player data not loaded.");
            return;
        }

        // Poll ID
        string pollId = Guid.NewGuid().ToString();
        List<string> voterIds = clan.Members.Select(m => m.Id).ToList();

        Sprite placeholderSprite = null; 
        long pollDurationMinutes = 1;

        var poll = new RolePollData(pollId, placeholderSprite, voterIds, RolePollType.Give, targetRole.ToString(), targetPlayerId, pollDurationMinutes);

        clan.Polls.Add(poll);
        store.SaveClanData(clan, updated => Debug.Log("Clan promotion poll created and saved."));
    }

    // Called when a poll ends (WIP)
    public static void EndPoll(string pollId)
    {
        Debug.Log("Claanin endpoll aktivoitu");

        var store = Storefront.Get();
        PlayerData player = null;
        ClanData clan = null;

        // Retrieve data for the current player
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);
        store.GetClanData(player.ClanId, data => clan = data);

        if (clan == null) return;

        var poll = clan.Polls.FirstOrDefault(p => p.Id == pollId);
        if (poll == null) return;

        bool passed = poll.YesVotes.Count > poll.NoVotes.Count;

        // Handle result for the passed poll
        if (poll is RolePollData rolePoll && passed)
        {
            var member = clan.Members.FirstOrDefault(m => m.Id == rolePoll.PlayerId);
            if (member != null && Enum.TryParse(rolePoll.RoleId, out ClanMemberRole parsedRole))
            {
                member.Role = parsedRole;
                Debug.Log($"Poll passed: promoted {member.Name} to {parsedRole}");
            }
            else
            {
                Debug.LogWarning("Member not found or role parse failed.");
            }
        }
        // Handle result if the poll didn't pass
        else if (poll is ClanRolePollData clanRolePoll && passed)
        {
            var member = clan.Members.FirstOrDefault(m => m.Id == clanRolePoll.TargetPlayerId);
            if (member != null)
            {
                member.Role = clanRolePoll.TargetRole;
                Debug.Log($"Poll passed: promoted {member.Name} to {clanRolePoll.TargetRole}");
            }
            else
            {
                Debug.LogWarning("Member not found in ClanRolePollData.");
            }
        }

        clan.Polls.Remove(poll);

        store.SaveClanData(clan, updated =>
        {
            Debug.Log("Poll ended and clan data saved.");
        });

        Debug.Log("Claanin endpoll loppui");
    }

}
