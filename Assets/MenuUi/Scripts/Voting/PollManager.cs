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
using MenuUI.Scripts;

public static class PollManager // Handles the polls from creation to loading to ending them
{
    private static List<PollData> pollDataList = new List<PollData>();
    private static List<PollData> pastPollDataList = new List<PollData>();

    private static DataStore store = Storefront.Get();
    private static PlayerData player = null;
    private static ClanData clan = null;

    public static Action<FurniturePollType> ShowVotingPopup;
    public static event Action OnPollCreated;

    private static KojuTrayPopulator trayPopulator;

    public static void RegisterTrayPopulator(KojuTrayPopulator populator)
    {
        trayPopulator = populator;
    }

    // Create poll for GameFurniture
    public static void CreateShopFurniturePoll(FurniturePollType furniturePollType, GameFurniture furniture, Action<bool> callback)
    {
        ServerManager.Instance.BuyShopItem(furniture.Name, result =>
        {
            if (result)
            {
                ShowVotingPopup?.Invoke(furniturePollType);

                OnPollCreated?.Invoke();
                if (callback != null)
                    callback(true);
            }
            else
            {
                SignalBus.OnChangePopupInfoSignal("Tavaran ostoäänestyksen luominen epäonnistui.");
                if (callback != null)
                    callback(false);
            }
        });
    }

    // Create poll for GameFurniture
    public static void CreateBuyFurniturePoll(FurniturePollType furniturePollType, GameFurniture furniture, string id, bool fetchData = false)
    {
        if(fetchData)LoadClanData();


        DataStore store = Storefront.Get();
        PlayerData player = null;
        ClanData clan = null;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player != null && player.ClanId != null)
        {
            store.GetClanData(player.ClanId, data => clan = data);
        }

        List<string> clanMembers = new List<string>();
        if (clan.Members != null) clanMembers = clan.Members.Select(member => member.Id).ToList();

        PollData pollData = new FurniturePollData(id, clanMembers, furniturePollType, furniture);
        //currentPollDataList.Add(pollData);

        // PrintPollList();
        if (fetchData) SaveClanData();

        PollMonitor.Instance?.StartMonitoring();

        OnPollCreated?.Invoke();
    }

    // Create poll for StorageFurniture
    public static void CreateFurnitureSellPoll(FurniturePollType furniturePollType, StorageFurniture furniture)
    {
        ServerManager.Instance.SellItemOnStall(furniture.Id, (int)furniture.Value, callback => {
            
            ShowVotingPopup?.Invoke(furniturePollType);

            OnPollCreated?.Invoke();
        });
    }

    public static void CreateSellFurniturePoll(FurniturePollType furniturePollType, StorageFurniture furniture, string id, bool fetchData = false)
    {
        if (fetchData) LoadClanData();

        DataStore store = Storefront.Get();
        PlayerData player = null;
        ClanData clan = null;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player != null && player.ClanId != null)
        {
            store.GetClanData(player.ClanId, data => clan = data);
        }

        List<string> clanMembers = new List<string>();
        if (clan.Members != null) clanMembers = clan.Members.Select(member => member.Id).ToList();

        GameFurniture gameFurniture = null;
        store.GetAllGameFurnitureYield(result => gameFurniture = result.First(item => item.Name == furniture.Name));

        PollData pollData = new FurniturePollData(id, clanMembers, furniturePollType, gameFurniture);
        //currentPollDataList.Add(pollData);

        //PrintPollList();
        if (fetchData) SaveClanData();

        PollMonitor.Instance?.StartMonitoring();

        OnPollCreated?.Invoke();
    }

    // Create poll for Role Promotion
    public static void CreateRolePromotionPoll(string targetPlayerId, ClanMemberRole targetRole)
    {
        LoadClanData();

        if (clan == null || player == null)
        {
            Debug.LogWarning("Clan or player data not loaded.");
            return;
        }

        string pollId = GetFirstAvailableId();

        List<string> voterIds = clan.Members.Select(m => m.Id).ToList();
        Sprite placeholderSprite = null;
        long pollDurationMinutes = 1;

        var poll = new ClanRolePollData(pollId, placeholderSprite, voterIds, pollDurationMinutes, targetPlayerId, targetRole);

        pollDataList.Add(poll);
        SaveClanData();

        Debug.Log($"Role promotion poll created for {targetPlayerId} {targetRole}");

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
        LoadClanData();

        PollData pollData = GetPollData(pollId);
        if (pollData == null)
        {
            Debug.LogWarning($"PollData not found for pollId: {pollId}");
            return;
        }

        // Calculate yes votes threshold (33% of total clan members) (Proper Vote Counting)
        int requiredYesVotes = Mathf.CeilToInt(clan.Members.Count * 0.33f);
        int yesCount = pollData.YesVotes.Count;
        int noCount = pollData.NoVotes.Count;

        // Poll passes only if both conditions are true
        bool votePassed = yesCount >= requiredYesVotes && yesCount > noCount;
        // Proper poll calculation ends here

        // Simple Poll Vote Counting (Testing Only)
        //int yesCount = pollData.YesVotes.Count;
        //int noCount = pollData.NoVotes.Count;

        //bool votePassed = yesCount > noCount;
        // Simple Poll Vote Counting ends here

        // Handle ClanRolePollData
        if (pollData is ClanRolePollData clanRolePoll)
        {
            // Use the proper votePassed calculation instead of the simple count comparison
            if (votePassed)
            {
                var member = clan.Members.FirstOrDefault(m => m.Id == clanRolePoll.TargetPlayerId);
                if (member != null)
                {
                    member.Role = clanRolePoll.TargetRole.ToString();
                    Debug.Log($"Poll passed: promoted {member.Name} to {clanRolePoll.TargetRole}");
                }
                else
                {
                    Debug.LogWarning("Member not found in ClanRolePollData.");
                }
            }
            else
            {
                Debug.Log($"Clan role poll for {clanRolePoll.TargetPlayerId} did not pass. Yes votes: {yesCount}, required: {requiredYesVotes}"); // Change this into a comment when debugging polls without the member count restriction
            }
        }

        // Handle FurniturePollData and other polls
        else if (pollData is FurniturePollData furniturePollData)
        {
            int idx = clan.Inventory.Furniture.FindIndex(furn => furn.GameFurnitureName == furniturePollData.Furniture.Name);
            if (idx < 0)
            {
                Debug.LogWarning($"Furniture not found for poll: {furniturePollData.Furniture.Name}");
            }
            else
            {
                var clanFurniture = clan.Inventory.Furniture[idx];

                if (furniturePollData.FurniturePollType == FurniturePollType.Selling)
                {
                    clanFurniture.VotedToSell = votePassed;
                    clanFurniture.InVoting = false;

                    if (!votePassed)
                    {
                        Debug.Log($"Furniture poll for {furniturePollData.Furniture.Name} did not pass. Yes votes: {yesCount}, required: {requiredYesVotes}");
                    }
                }

                clan.Inventory.Furniture[idx] = clanFurniture;

                Debug.Log($"After update - Furniture: {clanFurniture.GameFurnitureName}, VotedToSell: {clanFurniture.VotedToSell}, InVoting: {clanFurniture.InVoting}"); // Change this into a comment when debugging polls without the member count restriction
            }
        }

        // Move poll from active to past
        pollDataList.Remove(pollData);
        pastPollDataList.Add(pollData);

        // Save clan data and refresh UI
        DataStore store = Storefront.Get();
        store.SaveClanData(clan, savedClan =>
        {
            clan = savedClan;
            Debug.Log("Clan data saved after ending poll.");

            // Refresh UI
            trayPopulator?.RefreshTray();
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
