using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Photon.Realtime;
using UnityEngine;

namespace Altzone.Scripts.Voting
{
    public enum PollType
    {
        Kauppa,
        Kirpputori,
        MemberPromote,
        MemberKick
    }

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

    public class PollData
    {
        public PollType PollType;
        public string Id;
        public string Name;
        public long StartTime;
        public long EndTime;
        public Sprite Sprite;

        public List<string> NotVoted;
        public List<PollVoteData> YesVotes;
        public List<PollVoteData> NoVotes;

        public PollData(PollType pollType, string id, string name,long startTime, long endTime, Sprite sprite, List<string> clanMembers)
        {
            PollType = pollType;
            Id = id;
            Name = name;
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
        public double Weight;
        public float Value;

        public FurniturePollData(PollType pollType, string id, string name, long startTime, long endTime, Sprite sprite, List<string> clanMembers, FurniturePollType furniturePollType, GameFurniture furniture, double weight, float value)
        : base(pollType, id, name, startTime, endTime, sprite, clanMembers)
        {
            FurniturePollType = furniturePollType;
            Furniture = furniture;
            Weight = weight;
            Value = value;
        }
    }

    public class EsinePollData : PollData
    {
        public EsinePollType EsinePollType;
        public float Value;

        public EsinePollData(PollType pollType, string id, string name, long startTime, long endTime, Sprite sprite, List<string> clanMembers, EsinePollType esinePollType, float value)
        : base(pollType, id, name, startTime, endTime, sprite, clanMembers)
        {
            EsinePollType = esinePollType;
            Value = value;
        }
    }

    //public class GiveRolePollData : PollData
    //{
    //    
    //}
}
