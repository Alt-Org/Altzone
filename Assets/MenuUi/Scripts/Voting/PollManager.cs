using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Voting;
using Altzone.Scripts;
using UnityEngine;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using UnityEditor;
using System.Linq;

public static class PollManager
{
    private static List<PollData> pollDataList = new List<PollData>();

    private static DataStore store = Storefront.Get();
    private static PlayerData player = null;
    private static ClanData clan = null;

    public static void CreatePoll(PollType pollType, int durationInHours, Sprite sprite, EsinePollType esinePollType, float value)
    {
        LoadClanData();

        string id = GetFirstAvailableId();
        string name = "joku";
        long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + durationInHours * 3600;

        List<string> clanMembers = new List<string>();
        //if (clan != null ) clanMembers = clan.Members.Select(member => member.Id).ToList();
        if (player != null) clanMembers.Add(player.Id);

        PollData pollData = new EsinePollData(pollType, id, name, endTime, sprite, clanMembers, esinePollType, value);

        pollDataList.Add(pollData);

        PrintPollList();
        SaveClanData();
    }

    public static void CreatePoll(PollType pollType, int durationInHours, Sprite sprite, FurniturePollType furniturePollType, GameFurniture furniture, double weight, float value)
    {
        LoadClanData();

        string id = GetFirstAvailableId();
        string name = "joku";
        long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + durationInHours * 3600;

        List<string> clanMembers = new List<string>();
        //if (clan.Members != null) clanMembers = clan.Members.Select(member => member.Id).ToList();
        if (player != null) clanMembers.Add(player.Id);

        PollData pollData = new FurniturePollData(pollType, id, name, endTime, sprite, clanMembers, furniturePollType, furniture, weight, value);    
        pollDataList.Add(pollData);

        PrintPollList();
        SaveClanData();
    }

    private static void PrintPollList()
    {
        for (int i = 0; i < pollDataList.Count; i++)
        {
            PollData pollData = pollDataList[i];

            DateTime dateTimeEnd = DateTimeOffset.FromUnixTimeSeconds(pollData.EndTime).DateTime;

            // Basic information common to all PollDatas
            string output = $"Poll Data {i + 1}: ID={pollData.Id}, Name={pollData.Name}, Time={dateTimeEnd}, Type={pollData.PollType}";

            // Check the type of pollData and append specific information
            if (pollData is FurniturePollData furniturePoll)
            {
                output += $", FurniturePollType={furniturePoll.FurniturePollType}, Weight={furniturePoll.Weight}, Value={furniturePoll.Value}";
            }
            else if (pollData is EsinePollData esinePoll)
            {
                output += $", EsinePollType={esinePoll.EsinePollType}, Value={esinePoll.Value}";
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
}
