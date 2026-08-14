using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Voting;
using UnityEngine;
using static MQTTManager;

public class PollMonitor : MonoBehaviour // Monitors active polls to check if they should be expired
{
    public static PollMonitor Instance { get; private set; }

    private Coroutine checkRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        ServerManager.OnClanPollsChanged += BuildPolls;
        MQTTManager.OnFurnitureBuyReceived += UpdatePollListAfterBuy;
        MQTTManager.OnFurnitureSellPollCreatedReceived += UpdatePollListAfterSell;
        MQTTManager.OnFurnitureStallBuyReceived += UpdatePollListChanged;
        MQTTManager.OnFurnitureStallBuyReceived += UpdatePollListAfterStallBuy;
    }

    private void OnDestroy()
    {
        ServerManager.OnClanPollsChanged -= BuildPolls;
        MQTTManager.OnFurnitureBuyReceived -= UpdatePollListAfterBuy;
        MQTTManager.OnFurnitureSellPollCreatedReceived -= UpdatePollListAfterSell;
        MQTTManager.OnFurnitureStallBuyReceived -= UpdatePollListChanged;
        MQTTManager.OnFurnitureStallBuyReceived -= UpdatePollListAfterStallBuy;
    }

    private void BuildPolls() => PollManager.BuildPolls();

    // Start monitoring when a poll begins
    public void StartMonitoring()
    {
        if (checkRoutine == null)
        {
            checkRoutine = StartCoroutine(CheckExpiredPollsRoutine());
            Debug.Log("Start Monitoring");
            PollManager.DebugPrintAllActivePolls();
        }
    }

    // Stop monitoring when there are no polls left
    public void StopMonitoring()
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
            Debug.Log("Stop Monitoring");
        }
    }

    // Expire polls that have run out, checking them every 5 seconds. If no polls are active, stop the coroutine for checking and stop monitoring
    private IEnumerator CheckExpiredPollsRoutine()
    {
        while (true)
        {
            PollManager.CheckAndExpirePolls();

            if (PollManager.GetPollList().Count == 0)
            {
                StopMonitoring();
                yield break;
            }

            Debug.Log("[PollMonitor] Checking for expired polls");
            yield return new WaitForSeconds(5f);
        }
    }

    private void UpdatePollListAfterBuy(MQTTFurnitureNotification buyFurniture)
    {
        VotingActions.ReloadPollList?.Invoke();

        AddFurniturePoll(buyFurniture);

        PollManager.ShowVotingPopup?.Invoke(FurniturePollType.Buying);
    }
    private void UpdatePollListAfterSell(MQTTFurnitureNotification sellFurniture)
    {
        VotingActions.ReloadPollList?.Invoke();
        AddFurniturePoll(sellFurniture);
        PollManager.ShowVotingPopup?.Invoke(FurniturePollType.Selling);
    }
    private void UpdatePollListChanged(MQTTFurnitureNotification changedFurniture)
    {
        VotingActions.ReloadPollList?.Invoke();
        //PollManager.ShowVotingPopup?.Invoke(FurniturePollType.Buying);
    }

    private void UpdatePollListAfterStallBuy(MQTTFurnitureNotification stallBuyFurniture)
    {
        VotingActions.ReloadPollList?.Invoke();
        AddFurniturePoll(stallBuyFurniture);
        PollManager.ShowVotingPopup?.Invoke(FurniturePollType.Buying);
    }

    private void AddFurniturePoll(MQTTFurnitureNotification buyFurniture)
    {
        ClanData clanData = null;

        Storefront.Get().GetClanData(ServerManager.Instance.Player.clan_id, data =>
        {
            clanData = data;
        });

        if (clanData != null)
        {
            ServerPoll poll = new()
            {
                _id = buyFurniture.voting_id,
                organizer = new()
                {
                    player_id = buyFurniture.organizer._id,
                },
                startedAt = DateTime.UtcNow.ToString(),
                endsOn = DateTime.UtcNow.AddMinutes(10).ToString(),
                type = buyFurniture.type,
                votes = new PollVote[0],
                shopItemName = string.IsNullOrEmpty(buyFurniture.entity.name)? buyFurniture.entity.shopItemName : buyFurniture.entity.name
            };

            FurniturePollData pollData = new FurniturePollData(poll, clanData);
            if (pollData.Furniture == null) return;

            clanData.Polls.Add(pollData);

        }

        Debug.Log("Poll added. ClanData.Polls count: " + clanData.Polls.Count);

        Storefront.Get().SaveClanData(clanData, null);
        ServerManager.Instance.RaiseClanPollsChangedEvent();
    }
}
