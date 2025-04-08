using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Voting;
using Altzone.Scripts;
using UnityEngine;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using System.Linq;
using MenuUi.Scripts.Storage;
using System.Collections.ObjectModel;

public static class PollManager
{
    private static List<PollData> pollDataList = new List<PollData>();

    private static DataStore store = Storefront.Get();
    private static PlayerData player = null;
    private static ClanData clan = null;

    public static void CreateFurniturePoll(FurniturePollType furniturePollType, GameFurniture furniture)
    {
        LoadClanData();

        string id = GetFirstAvailableId();

        List<string> clanMembers = new List<string>();
        if (clan.Members != null) clanMembers = clan.Members.Select(member => member.Id).ToList();

        PollData pollData = new FurniturePollData(id, clanMembers, furniturePollType, furniture);
        pollDataList.Add(pollData);

        //PrintPollList();
        SaveClanData();
    }

    public static void CreateFurniturePoll(FurniturePollType furniturePollType, StorageFurniture furniture)
    {
        LoadClanData();

        string id = GetFirstAvailableId();

        List<string> clanMembers = new List<string>();
        if (clan.Members != null) clanMembers = clan.Members.Select(member => member.Id).ToList();

        GameFurniture gameFurniture = null;
        store.GetAllGameFurnitureYield(result => gameFurniture = result.First(item => item.Name == furniture.Name));

        PollData pollData = new FurniturePollData(id, clanMembers, furniturePollType, gameFurniture);
        pollDataList.Add(pollData);

        //PrintPollList();
        SaveClanData();
    }

    private static void PrintPollList()
    {
        for (int i = 0; i < pollDataList.Count; i++)
        {
            PollData pollData = pollDataList[i];

            DateTime dateTimeEnd = DateTimeOffset.FromUnixTimeSeconds(pollData.EndTime).DateTime;

            // Basic information common to all PollDatas
            string output = $"Poll Data {i + 1}: ID={pollData.Id}, Time={dateTimeEnd}";

            // Check the type of pollData and append specific information
            if (pollData is FurniturePollData furniturePoll)
            {
                output += $", FurniturePollType={furniturePoll.FurniturePollType}, Weight={furniturePoll.Furniture.Weight}, Value={furniturePoll.Furniture.Value}";
            }

            Debug.Log(output);
        }
    }

    private static string GetFirstAvailableId()
    {
        string newId;
        do
        {
            newId = Guid.NewGuid().ToString();
        } while (pollDataList.Any(poll => poll.Id == newId));

        return newId;
    }

    public static List<PollData> GetPollList()
    {
        return pollDataList;
    }

    public static PollData GetPollData(string id)
    {
        return pollDataList.First(x => x.Id == id);
    }

    public static void LoadClanData()
    {
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player != null)
        {
            if (player.ClanId != null)
            {
                store.GetClanData(player.ClanId, data => clan = data);
                
                if (clan.Polls != null)
                {
                    pollDataList = clan.Polls;
                }
            }
        }
    }

    public static void SaveClanData()
    {
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player != null)
        {
            if (player.ClanId != null)
            {
                store.GetClanData(player.ClanId, data => clan = data);

                clan.Polls = pollDataList;
                store.SaveClanData(clan, data => clan = data);
            }
        }
    }

    public static void EndPoll(string pollId)
    {
        PollData pollData = GetPollData(pollId);

        bool yesVotesWon = (pollData.YesVotes.Count >= Mathf.CeilToInt(clan.Members.Count / 3.0f)) && pollData.YesVotes.Count > pollData.NoVotes.Count;

        if (pollData is FurniturePollData furniturePollData)
        {
            FurniturePollType pollType = furniturePollData.FurniturePollType;

            if (pollType == FurniturePollType.Selling)
            {
                ClanFurniture clanFurniture = clan.Inventory.Furniture.First(furn => furn.GameFurnitureName == furniturePollData.Furniture.Name);
                clanFurniture.VotedToSell = yesVotesWon;
                clanFurniture.InVoting = false;
            }
            // TODO: Buying
        }

        pollData.IsOver = true;
    }
}
