using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
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

        public bool IsExpired => EndTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds();


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

        public PollData(ServerPoll poll)
        {
            Id = poll._id;
            StartTime = poll.startedAt !=null ?((DateTimeOffset)DateTime.Parse(poll.startedAt).ToLocalTime()).ToUnixTimeSeconds(): 0;
            EndTime = ((DateTimeOffset)DateTime.Parse(poll.endsOn).ToLocalTime()).ToUnixTimeSeconds();

            List<string> clanMembers = new List<string>();
            ClanData clan = null;
            Storefront.Get().GetClanData(ServerManager.Instance.Player.clan_id, c => clan=c);
            if (clan.Members != null) clanMembers = clan.Members.Select(member => member.Id).ToList();

            NotVoted = clanMembers;
            YesVotes = new List<PollVoteData>();
            NoVotes = new List<PollVoteData>();
            foreach (PollVote vote in poll.votes)
            {
                if (vote.choice == "accept")
                {
                    if (NotVoted.Contains(vote.player_id))
                    {
                        YesVotes.Add(new(vote.player_id, clan.Members.Find(member => vote.player_id == member.Id)?.Name, true));
                        NotVoted.Remove(vote.player_id);
                    }
                }
                else if(vote.choice == "decline")
                {
                    if (NotVoted.Contains(vote.player_id))
                    {
                        NoVotes.Add(new(vote.player_id, clan.Members.Find(member => vote.player_id == member.Id)?.Name, true));
                        NotVoted.Remove(vote.player_id);
                    }
                }
            }
        }

        public void AddVote(bool answer)
        {
            DataStore store = Storefront.Get();
            PlayerData player = null;

            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

            // Method to check if the player has already voted. Interacting with the PollObject is already blocked in votemanager, but this can act as a failsafe for now
            if (player != null)
            {
                bool hasVoted = YesVotes.Any(vote => vote.PlayerId == player.Id) ||
                                NoVotes.Any(vote => vote.PlayerId == player.Id);
                if (hasVoted)
                {
                    return;
                }

                ServerManager.Instance.SendClanVoteToServer(Id, answer, callback =>
                {
                    if (answer)
                    {
                        PollVoteData newPollVote = new(player.Id, player.Name, answer);

                        if (NotVoted.Contains(player.Id))
                        {
                            if (answer) YesVotes.Add(newPollVote);
                            else NoVotes.Add(newPollVote);

                            NotVoted.Remove(player.Id);
                        }
                    }
                });
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
        
        public FurniturePollData(string id, List<string> clanMembers, FurniturePollType furniturePollType, GameFurniture furniture, long endTime = 1)
        : base(id, furniture.FurnitureInfo.Image, clanMembers, endTime)
        {
            FurniturePollType = furniturePollType;
            Furniture = furniture;
        }

        public FurniturePollData(ServerPoll poll ,ClanData clanData)
        : base(poll)
        {
            GameFurniture gameFurniture = null;
            if (poll.type == "flea_market_sell_item")
            {
                FurniturePollType = FurniturePollType.Selling;
                ClanFurniture clanFurniture = clanData.Inventory.Furniture.First(item => item.Id == poll.fleaMarketItem_id);
                Storefront.Get().GetAllGameFurnitureYield(result => gameFurniture = result.First(item => item.Name == clanFurniture.GameFurnitureName));
                if(!IsExpired) clanFurniture.InVoting = true;
            }
            else if(poll.type == "shop_buy_item")
            {
                FurniturePollType = FurniturePollType.Buying;
                Storefront.Get().GetAllGameFurnitureYield(result => gameFurniture = result.First(item => item.Name == poll.shopItemName));
            }
            Sprite = gameFurniture.FurnitureInfo.Image;
            Furniture = gameFurniture;
        }
    }

    public class RolePollData : PollData
    {
        public RolePollType RolePollType;
        public string RoleId;
        public string PlayerId;

        public RolePollData(string id, Sprite sprite, List<string> clanMembers, RolePollType rolePollType, string roleId, string playerId, long endTime = 1)
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

        public MemberPollData(string id, Sprite sprite, List<string> clanMembers, MemberPollType memberPollType, string playerId, long endTime = 1)
        : base(id, sprite, clanMembers, endTime)
        {
            MemberPollType = memberPollType;
            PlayerId = playerId;
        }
    }
}
