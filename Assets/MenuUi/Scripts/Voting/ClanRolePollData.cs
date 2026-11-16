using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;

namespace Altzone.Scripts.Voting
{
    [Serializable]
    public class ClanRolePollData : PollData
    {
        public string TargetPlayerId;
        public ClanMemberRole TargetRole;

        public ClanRolePollData(string id, Sprite icon, List<string> clanMembers, long endTime,
                                string targetPlayerId, ClanMemberRole targetRole)
            : base(id, icon, clanMembers, endTime)
        {
            TargetPlayerId = targetPlayerId;
            TargetRole = targetRole;
        }
    }

    [Serializable]
    public class ClanRoleCreatePollData : PollData
    {
        public string ProposerPlayerId;
        public ClanRoles NewRole;

        public ClanRoleCreatePollData(string id, Sprite icon, List<string> clanMembers, long endTime,
                                      string targetId, ClanRoles newRole)
            : base(id, icon, clanMembers, endTime)
        {
            NewRole = newRole;
        }
    }
}
