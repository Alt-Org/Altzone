using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
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

        public PollData(string id, Sprite sprite, List<string> clanMembers, long endTime)
        {
            Id = id;
            StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            EndTime = StartTime + endTime * 60;
            Sprite = sprite;
            NotVoted = clanMembers;
            YesVotes = new List<PollVoteData>();
            NoVotes = new List<PollVoteData>();
        }

        public void AddVote(bool answer)
        {
            DataStore store = Storefront.Get();
            PlayerData player = null;
            
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

            if (player != null)
            {
                PollVoteData previousPollVote = null;
                if (answer) previousPollVote = NoVotes.FirstOrDefault(vote => vote.PlayerId == player.Id);
                else previousPollVote = YesVotes.FirstOrDefault(vote => vote.PlayerId == player.Id);

                PollVoteData newPollVote = new(player.Id, player.Name, answer);

                if (NotVoted.Contains(player.Id))
                {
                    if (answer) YesVotes.Add(newPollVote);
                    else NoVotes.Add(newPollVote);

                    NotVoted.Remove(player.Id);
                }
                else if (previousPollVote != null) // Has already voted on this poll
                {
                    if (previousPollVote.Answer == true && answer == false)
                    {
                        YesVotes.Remove(previousPollVote);
                        NoVotes.Add(newPollVote);
                    }
                    else if (previousPollVote.Answer == false && answer == true)
                    {
                        NoVotes.Remove(previousPollVote);
                        YesVotes.Add(newPollVote);
                    }
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
        
        public FurniturePollData(string id, List<string> clanMembers, FurniturePollType furniturePollType, GameFurniture furniture, long endTime = 15)
        : base(id, furniture.FurnitureInfo.Image, clanMembers, endTime)
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

        public RolePollData(string id, Sprite sprite, List<string> clanMembers, RolePollType rolePollType, string roleId, string playerId, long endTime = 15)
        : base(id, sprite, clanMembers, endTime)
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

        public MemberPollData(string id, Sprite sprite, List<string> clanMembers, MemberPollType memberPollType, string playerId, long endTime = 15)
        : base(id, sprite, clanMembers, endTime)
        {
            MemberPollType = memberPollType;
            PlayerId = playerId;
        }
    }
}
