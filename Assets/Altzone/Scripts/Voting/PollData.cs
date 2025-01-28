using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Newtonsoft.Json.Linq;
using Photon.Realtime;
using UnityEngine;

namespace Altzone.Scripts.Voting
{
    public enum FurniturePollType
    {
        Buying,
        Selling,
        Recycling
    }

    public enum EsinePollType
    {
        Buying,
        Selling
    }

    public enum RolePollType
    {
        Create,
        Delete,
        Modify,
        Give,
    }

    public enum MemberPollType
    {
        Accept,
        Kick
    }

    public class PollData
    {
        public string Id;
        public long StartTime;
        public long EndTime;
        public Sprite Sprite;

        public List<string> NotVoted;
        public List<PollVoteData> YesVotes;
        public List<PollVoteData> NoVotes;

        public PollData(string id,long startTime, long endTime, Sprite sprite, List<string> clanMembers)
        {
            Id = id;
            StartTime = startTime;
            EndTime = endTime;
            Sprite = sprite;
            NotVoted = new List<string>();
            YesVotes = new List<PollVoteData>();
            NoVotes = new List<PollVoteData>();
        }

        public void AddVote(bool answer)
        {
            DataStore store = Storefront.Get();
            PlayerData player = null;
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

            string playerId = null;
            string playerName = null;
            if (player != null) playerId = player.Id;
            if (player != null) playerName = player.Name;

            if (NotVoted.Contains(playerId) || true) //temporarily true for testing
            {
                if (answer)
                {
                    PollVoteData newPollVote = new PollVoteData(playerId, playerName, answer);
                    YesVotes.Add(newPollVote);
                }
                else
                {
                    PollVoteData newPollVote = new PollVoteData(playerId, playerName, answer);
                    NoVotes.Add(newPollVote);
                }
            }

            //PlayerVoteData newPlayerVote = new PlayerVoteData(Id, answer);
            //player.playerVotes.Add(newPlayerVote);
            //store.SavePlayerData(player, data => player = data);
        }
    }


    public class FurniturePollData : PollData
    {
        public FurniturePollType FurniturePollType;
        public GameFurniture Furniture;

        public FurniturePollData(string id, long startTime, long endTime, Sprite sprite, List<string> clanMembers, FurniturePollType furniturePollType, GameFurniture furniture)
        : base(id, startTime, endTime, sprite, clanMembers)
        {
            FurniturePollType = furniturePollType;
            Furniture = furniture;
        }
    }

    public class RolePollData : PollData
    {
        public RolePollType RolePollType;
        public string RoleId;
        public string PlayerId;

        public RolePollData(string id, long startTime, long endTime, Sprite sprite, List<string> clanMembers, RolePollType rolePollType, string roleId, string playerId)
        : base(id, startTime, endTime, sprite, clanMembers)
        {
            RolePollType = rolePollType;
            RoleId = roleId;
            PlayerId = playerId;
        }
    }

    public class MemberPollData : PollData
    {
        public MemberPollType MemberPollType;
        public string PlayerId;

        public MemberPollData(string id, long startTime, long endTime, Sprite sprite, List<string> clanMembers, MemberPollType memberPollType, string playerId)
        : base(id, startTime, endTime, sprite, clanMembers)
        {
            MemberPollType = memberPollType;
            PlayerId = playerId;
        }
    }
}
