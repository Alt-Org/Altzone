using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MQTTManager : MonoBehaviour
{
    public static MQTTManager Instance { get; private set; }
    public IMqttClient Client => _client; 

    private IMqttClient _client = null;

    public static bool IsConnected
    {
        get
        {
            return Instance.Client == null || !Instance.Client.IsConnected;
        }
    }

    private string _votingTopic = null;
    private string _dailyTaskPlayerTopic = null;
    private string _dailyTaskClanTopic = null;
    private string _matchmakingInviteTopic = null;
    private string _jukeboxTopic = null;

    private const string VotingTopicBase = "/clan/{clanId}/voting/+/+";
    private const string DailyTaskPlayerTopicBase = "/player/{playerId}/daily_task/+/+";
    private const string DailyTaskClanTopicBase = "/clan/{clanId}/daily_task/+/+";
    private const string MatchmakingInviteTopicBase = "/matchmaking/invites/player/{playerId}";
    private const string MatchmakingMatchTopicBase = "/matchmaking/match/player/{playerId}";
    private const string JukeboxTopicBase = "/clan/{clanId}/jukebox/+/update";

    private List<MqttApplicationMessageReceivedEventArgs> _pendingNotificationList = new();

    public delegate void MQTTConnectionEstablished(bool established);
    public static event MQTTConnectionEstablished OnMQTTConnectionEstablished;

    public delegate void VoteNotificationReceived();
    public static event VoteNotificationReceived OnVoteNotificationReceived;

    public delegate void MatchmakingInviteReceived(MqttMatchInvite invite);
    public static event MatchmakingInviteReceived OnMatchmakingInviteReceived;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private async void OnDestroy()
    {
        if (_client != null && _client.IsConnected)
        {
            await _client.DisconnectAsync();
        }
    }
    private void Update()
    {
        int pendingCount = _pendingNotificationList.Count;
        if (pendingCount > 0)
        {
            for(int i = 0; pendingCount > i; i++)
            {
                ParsePayload(_pendingNotificationList[i]);
            }
            for (int i = pendingCount - 1; i >= 0; i--)
            {
                _pendingNotificationList.RemoveAt(i);
            }
        }
    }

    public async void StartMQTT()
    {
        await StartMQTTAsync();
    }

    private async Task StartMQTTAsync()
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _client.ApplicationMessageReceivedAsync += (e) =>
        {
            _pendingNotificationList.Add(e);
            return Task.CompletedTask;
        };

        _client.ConnectedAsync += e =>
        {
            Debug.Log("MQTT connected");
            return Task.CompletedTask;
        };

        _client.DisconnectedAsync += e =>
        {
            Debug.LogWarning($"MQTT disconnected: {e.Reason}");
            OnMQTTConnectionEstablished?.Invoke(false);
            return Task.CompletedTask;
        };

        var options = new MqttClientOptionsBuilder()
            .WithWebSocketServer(o => o.WithUri("ws://notifications.altzone.fi"))
            .WithCredentials("subscriber", "QNecDttbY92MzfURzPzOjYvICnBkmAXI")
            .WithClientId(ServerManager.Instance.Player._id)
            .WithCleanSession()
            .Build();

        try
        {
            Debug.Log($"Connecting to ws://notifications.altzone.fi");
            await _client.ConnectAsync(options);
        }
        catch (Exception ex)
        {
            Debug.LogError($"MQTT connection failed: {ex}");
        }
        finally
        {
            //if (_client != null && _client.IsConnected)
                //OnMQTTConnectionEstablished?.Invoke(true);

            if(ServerManager.Instance.Clan != null) SubscribeToClanNotifications();
        }
    }

    public async void SubscribeToClanNotifications()
    {
        await SubscribeToVoting();
        await SubscribeToDailyTask();
        await SubscribeToJukebox();
        await SubscribeToMatchmaking();
        OnMQTTConnectionEstablished?.Invoke(true);
    }

    public async void UnsubscribeFromClanNotifications()
    {
        await UnsubscribeFromVoting();
        await UnsubscribeFromDailyTask();
        await UnsubscribeFromJukebox();
        await UnsubscribeFromMatchmaking();

    }

    public async Task SubscribeToVoting()
    {
        if (_client == null || !_client.IsConnected || _votingTopic != null) return;

        _votingTopic = VotingTopicBase.Replace("{clanId}", ServerManager.Instance.Clan._id);

        try
        {
            Debug.Log($"Subscribing to {_votingTopic}");
            await _client.SubscribeAsync(_votingTopic);
            Debug.Log($"Subscribtion to {_votingTopic} successful");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Subscription failed: {ex}");
        }
    }

    public async Task UnsubscribeFromVoting()
    {
        if (_client == null || !_client.IsConnected)
        {
            _votingTopic = null;
            return;
        }

        try
        {
            Debug.Log($"Unsubscribing from {_votingTopic}");
            await _client.UnsubscribeAsync(_votingTopic);
            Debug.Log($"Unsubscribtion from {_votingTopic} successful");

            _votingTopic = null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Subscription failed: {ex}");
        }
    }

    public async Task SubscribeToDailyTask()
    {
        if (_client == null || !_client.IsConnected || _dailyTaskPlayerTopic != null) return;

        _dailyTaskPlayerTopic = DailyTaskPlayerTopicBase.Replace("{playerId}", ServerManager.Instance.Player._id);
        _dailyTaskClanTopic = DailyTaskClanTopicBase.Replace("{clanId}", ServerManager.Instance.Clan._id);

        try
        {
            Debug.Log($"Subscribing to {_dailyTaskPlayerTopic}");
            await _client.SubscribeAsync(_dailyTaskPlayerTopic);
            Debug.Log($"Subscribtion to {_dailyTaskPlayerTopic} successful");

            Debug.Log($"Subscribing to {_dailyTaskClanTopic}");
            await _client.SubscribeAsync(_dailyTaskClanTopic);
            Debug.Log($"Subscribtion to {_dailyTaskClanTopic} successful");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Subscription failed: {ex}");
        }
    }

    public async Task UnsubscribeFromDailyTask()
    {
        if (_client == null || !_client.IsConnected)
        {
            _dailyTaskPlayerTopic = null;
            _dailyTaskClanTopic = null;
            return;
        }

        try
        {
            Debug.Log($"Unsubscribing from {_dailyTaskPlayerTopic}");
            await _client.UnsubscribeAsync(_dailyTaskPlayerTopic);
            Debug.Log($"Unsubscribtion from {_dailyTaskPlayerTopic} successful");

            _dailyTaskPlayerTopic = null;

            Debug.Log($"Unsubscribing from {_dailyTaskClanTopic}");
            await _client.UnsubscribeAsync(_dailyTaskClanTopic);
            Debug.Log($"Unsubscribtion from {_dailyTaskClanTopic} successful");

            _dailyTaskClanTopic = null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Subscription failed: {ex}");
        }
    }

    public async Task SubscribeToMatchmaking()
    {
        if (_client == null || !_client.IsConnected || _matchmakingInviteTopic != null) return;

        _matchmakingInviteTopic = MatchmakingInviteTopicBase.Replace("{playerId}", ServerManager.Instance.Player._id);
       // var topic2 = $"/matchmaking/matches/player/{ServerManager.Instance.Player._id}";

        try
        {
            Debug.Log($"Subscribing to {_matchmakingInviteTopic}");
            await _client.SubscribeAsync(_matchmakingInviteTopic);
            Debug.Log($"Subscribtion to {_matchmakingInviteTopic} successful");

            //Debug.Log($"Subscribing to {topic2}");
            //await _client.SubscribeAsync(topic2);
            //Debug.Log($"Subscribtion to {topic2} successful");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Subscription failed: {ex}");
        }
    }

    public async Task UnsubscribeFromMatchmaking()
    {
        if (_client == null || !_client.IsConnected)
        {
            _matchmakingInviteTopic = null;
            return;
        }

        try
        {
            Debug.Log($"Unsubscribing from {_matchmakingInviteTopic}");
            await _client.UnsubscribeAsync(_matchmakingInviteTopic);
            Debug.Log($"Unsubscribtion from {_matchmakingInviteTopic} successful");

            _matchmakingInviteTopic = null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unsubscription failed: {ex}");
        }
    }

    public async Task SubscribeToJukebox()
    {
        if (_client == null || !_client.IsConnected || _jukeboxTopic != null) return;

        _jukeboxTopic = JukeboxTopicBase.Replace("{clanId}", ServerManager.Instance.Clan._id);

        try
        {
            Debug.Log($"Subscribing to {_jukeboxTopic}");
            await _client.SubscribeAsync(_jukeboxTopic);
            Debug.Log($"Subscribtion to {_jukeboxTopic} successful");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Subscription failed: {ex}");
        }
    }

    public async Task UnsubscribeFromJukebox()
    {
        if (_client == null || !_client.IsConnected)
        {
            _jukeboxTopic = null;
            return;
        }

        try
        {
            Debug.Log($"Unsubscribing from {_jukeboxTopic}");
            await _client.UnsubscribeAsync(_jukeboxTopic);
            Debug.Log($"Unsubscribtion from {_jukeboxTopic} successful");

            _jukeboxTopic = null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unsubscription failed: {ex}");
        }
    }

    private void ParsePayload(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var message = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
            var topic = args.ApplicationMessage.Topic;
            Debug.Log($"MQTT message received. Topic: {topic}");
            Debug.Log($"Payload: {message}");
            JObject result = JObject.Parse(message);
            if (topic.Split("/")[1].Equals("matchmaking"))
            {
                if (result["type"].ToString().Equals("INVITE_UPDATED"))
                {
                    MqttMatchInvite invite = result["payload"].ToObject<MqttMatchInvite>();

                    OnMatchmakingInviteReceived?.Invoke(invite);
                }
            }
            else if(topic.Split("/")[3].Equals("voting"))
            {
                Debug.Log($"Voting received: {message}");
                OnVoteNotificationReceived?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Payload parsing failed: {ex}");
        }
    }

    [Serializable]
    public class MqttMatchInvite
    {
        public string id { get; set; }
        public MatchType matchType { get; set; }
        public InviteStatus status { get; set; }
        public string ownerPlayreId { get; set; }
        public string clanId { get; set; }
        public string roomId { get; set; }
        public List<string> players { get; set; }
        public MQTTBots bots { get; set; }
        public int teamSize { get; set; }
        public bool allowBots { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public string readyAt { get; set; }
        public string matchId { get; set; }
    }

    [Serializable]
    public class MqttMatchSet
    {
        public string id { get; set; }
        public MatchType matchType { get; set; }
        public MatchStatus status { get; set; }
        public int teamSize { get; set; }
        public List<MQTTMatchTeams> teams { get; set; }
        public string startedAt { get; set; }
        public string finishedAt { get; set; }
        public MatchResult winningSide { get; set; }
    }

    public enum MatchType
    {
        RANDOM,
        CLAN,
        CUSTOM
    }

    public enum InviteStatus
    {
        OPEN,
        READY,
        QUEUED,
        MATCHED,
        CANCELLED
    }

    public enum MatchStatus
    {
        ACTIVE,
        FINISHED
    }

    [Serializable]
    public class MQTTMatchTeams
    {
        public string side { get; set; }
        public string clanId { get; set; }
        public List<MQTTMatchPlayers> players { get; set; }
        public List<MQTTBots> bots { get; set; }
    }

    [Serializable]
    public class MQTTMatchPlayers
    {
        public string playerId { get; set; }
        public string displayName { get; set; }
        public bool isBot { get; set; }
    }

    [Serializable]
    public class MQTTBots
    {
        public string botId { get; set; }
        public string displayName { get; set; }
        public bool isBot { get; set; }
    }

    [Serializable]
    public class MatchResult
    {
        public string winningSide { get; set; }
    }
}
