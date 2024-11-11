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

public static class PollManager
{
    private static List<PollData> pollDataList = new List<PollData>();

    public static void CreatePoll(PollType pollType, string id, int durationInHours, Sprite sprite, EsinePollType esinePollType, float value)
    {
        LoadPollList();
        string name = "joku";

        long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + durationInHours * 3600;

        PollData pollData = new EsinePollData(pollType, id, name, endTime, sprite, esinePollType, value);
        pollDataList.Add(pollData);

        PrintPollList();
        SavePollList();
    }

    public static void CreatePoll(PollType pollType, string id, int durationInHours, Sprite sprite, FurniturePollType furniturePollType, GameFurniture furniture, double weight, float value)
    {
        LoadPollList();
        string name = "joku";

        long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + durationInHours * 3600;

        PollData pollData = new FurniturePollData(pollType, id, name, endTime, sprite, furniturePollType, furniture, weight, value);
        pollDataList.Add(pollData);

        PrintPollList();
        SavePollList();
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

    public static List<PollData> GetPollList()
    {
        return pollDataList;
    }

    public static void LoadPollList()
    {
        DataStore store = Storefront.Get();
        PlayerData player = null;
        ClanData clan = null;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);
        store.GetClanData(player.ClanId, data => clan = data);

        pollDataList = clan.Polls;
    }

    public static void SavePollList()
    {
        DataStore store = Storefront.Get();
        PlayerData player = null;
        ClanData clan = null;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);
        store.GetClanData(player.ClanId, data => clan = data);

        clan.Polls = pollDataList;
        store.SaveClanData(clan, data => clan = data);
    }
}
