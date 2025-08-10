using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Voting;
using MenuUi.Scripts.Storage;

public static class PollManager // Handles the polls from creation to loading to ending them
{
    private static List<PollData> pollDataList = new List<PollData>();
    private static List<PollData> pastPollDataList = new List<PollData>();

    private static DataStore store = Storefront.Get();
    private static PlayerData player = null;
    private static ClanData clan = null;

    public static Action<FurniturePollType> ShowVotingPopup;

    // Create poll for GameFurniture
    public static void CreateFurniturePoll(FurniturePollType furniturePollType, GameFurniture furniture)
    {
        LoadClanData();

        string id = GetFirstAvailableId();

        List<string> clanMembers = new List<string>();
        if (clan.Members != null) clanMembers = clan.Members.Select(member => member.Id).ToList();

        PollData pollData = new FurniturePollData(id, clanMembers, furniturePollType, furniture);
        pollDataList.Add(pollData);

        ShowVotingPopup?.Invoke(furniturePollType);

        //PrintPollList();
        SaveClanData();

        PollMonitor.Instance?.StartMonitoring();
    }

    // Create poll for StorageFurniture
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

        ShowVotingPopup?.Invoke(furniturePollType);

        //PrintPollList();
        SaveClanData();

        PollMonitor.Instance?.StartMonitoring();
    }

    public static void BuildPolls(List<ServerPoll> polls)
    {
        LoadClanData();

        List<string> clanMembers = new List<string>();
        if (clan.Members != null) clanMembers = clan.Members.Select(member => member.Id).ToList();

        foreach (ServerPoll poll in polls)
        {

            PollData pollData = new FurniturePollData(poll);
            pollDataList.Add(pollData);
        }

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

    public static void DebugPrintAllActivePolls()
    {
        Debug.Log("----- Active Polls Start -----");
        foreach (var poll in pollDataList)
        {
            string furnitureName = "(unknown)";
            if (poll is FurniturePollData fPoll)
            {
                furnitureName = fPoll.Furniture?.Name ?? "(null furniture)";
            }
            Debug.Log($"Poll ID: {poll.Id}, Furniture: {furnitureName}, Expired: {poll.IsExpired}");
        }
        Debug.Log("----- Active Polls End -----");
    }


    // Create a unique ID for the poll
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
        return pollDataList.FirstOrDefault(x => x.Id == id);
    }

    public static void LoadClanData()
    {
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player != null && player.ClanId != null)
        {
            store.GetClanData(player.ClanId, data => clan = data);

            if (clan?.Polls != null)
            {
                pollDataList = clan.Polls;
            }
        }
    }

    public static void SaveClanData()
    {
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player != null && player.ClanId != null)
        {
            store.GetClanData(player.ClanId, data => clan = data);

            clan.Polls = pollDataList;
            store.SaveClanData(clan, data => clan = data);
        }
    }

    // Ends the poll and determines if it passed, and updates clan inventory accordingly (WIP)
    public static void EndPoll(string pollId)
    {
        Debug.Log("EndPoll Pollmanagerissa");

        LoadClanData();

        PollData pollData = GetPollData(pollId);
        if (pollData == null)
        {
            Debug.LogWarning($"PollData not found for pollId: {pollId}");
            return;
        }

        // Check if pollData is a clan role promotion poll 
        if (pollData is RolePollData || pollData is ClanRolePollData)
        {
            // Delegate to ClanPollManager's EndPoll instead (this should we reworked later)
            ClanPollManager.EndPoll(pollId);
            return; 
        }

        // Continue with the poll normally if it was not a role promotion poll
        int yesCount = pollData.YesVotes.Count;
        int noCount = pollData.NoVotes.Count;

        bool votePassed = yesCount > noCount;

        Debug.Log("EndPoll jatkui managerissa");

        FurniturePollData furniturePollData = null;
        if (pollData is FurniturePollData fpd)
        {
            furniturePollData = fpd;

            if (furniturePollData.FurniturePollType == FurniturePollType.Selling)
            {
                int idx = clan.Inventory.Furniture.FindIndex(furn => furn.GameFurnitureName == furniturePollData.Furniture.Name);

                if (idx < 0)
                {
                    Debug.LogWarning($"Furniture not found for poll: {furniturePollData.Furniture.Name}");
                    return;
                }

                var clanFurniture = clan.Inventory.Furniture[idx];

                Debug.Log($"Before update - Furniture: {clanFurniture.GameFurnitureName}, VotedToSell={clanFurniture.VotedToSell}, InVoting={clanFurniture.InVoting}");

                if (votePassed)
                {
                    clanFurniture.VotedToSell = true;
                    clanFurniture.InVoting = false;
                }
                else
                {
                    clanFurniture.VotedToSell = false;
                    clanFurniture.InVoting = false;
                }

                clan.Inventory.Furniture[idx] = clanFurniture;

                Debug.Log($"After update - VotedToSell: {clanFurniture.VotedToSell}, InVoting: {clanFurniture.InVoting}");
            }
        }

        pollDataList.Remove(pollData);
        pastPollDataList.Add(pollData);

        DataStore store = Storefront.Get();
        store.SaveClanData(clan, savedClan =>
        {
            clan = savedClan;
            Debug.Log("Clan data saved with updated VotedToSell and InVoting flags.");

            // Verify furniture flags after save
            var savedFurniture = clan.Inventory.Furniture.FirstOrDefault(f => f.GameFurnitureName == furniturePollData?.Furniture.Name);
            Debug.Log($"After save - Furniture VotedToSell: {savedFurniture?.VotedToSell}, InVoting: {savedFurniture?.InVoting}");

            VotingActions.ReloadPollList?.Invoke();
            PastPollManager.OnPastPollsChanged?.Invoke();

            if (pollDataList.Count == 0)
            {
                PollMonitor.Instance?.StopMonitoring();
            }
        });
    }


    // Checks for expired polls and ends those that have expired
    public static void CheckAndExpirePolls()
    {
        if (pollDataList == null || pollDataList.Count == 0) return;

        List<PollData> expiredPolls = pollDataList.Where(p => p.IsExpired).ToList();

        foreach (var poll in expiredPolls)
        {
            Debug.Log($"[CheckExpirePolls] Poll {poll.Id} expired? {poll.IsExpired}");
            EndPoll(poll.Id);
        }
    }

    public static PollData GetAnyPollData(string id)
    {
        var poll = pollDataList.FirstOrDefault(p => p.Id == id);
        if (poll != null) return poll;

        return pastPollDataList.FirstOrDefault(p => p.Id == id);
    }

    public static List<PollData> GetPastPollList()
    {
        return pastPollDataList;
    }
}
