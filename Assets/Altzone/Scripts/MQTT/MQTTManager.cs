using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Altzone.Scripts.MQTT
{
    public class MQTTManager : MonoBehaviour
    {
        public static MQTTManager Instance { get; private set; }
        public IMqttClient Client => _client;

        private IMqttClient _client = null;

        private bool _subscriptionsDone = false;

        public static bool IsConnected
        {
            get
            {
                return Instance.Client == null || !Instance.Client.IsConnected;
            }
        }

        public static bool IsSubscribed
        {
            get
            {
                return IsConnected && Instance._subscriptionsDone;
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

        public delegate void FurnitureSellPollCreatedReceived(MQTTFurnitureNotification sellFurniture);
        public static event FurnitureSellPollCreatedReceived OnFurnitureSellPollCreatedReceived;
        public delegate void FurnitureStallBuyReceived(MQTTFurnitureNotification sellFurniture);
        public static event FurnitureStallBuyReceived OnFurnitureStallBuyReceived;
        public delegate void FurnitureSellChangedReceived(MQTTFurnitureNotification sellFurniture);
        public static event FurnitureSellChangedReceived OnFurnitureSellChangedReceived;
        public delegate void FurnitureBuyReceived(MQTTFurnitureNotification sellFurniture);
        public static event FurnitureBuyReceived OnFurnitureBuyReceived;

        public delegate void VoteReceived(MQTTVotingUpdatedNotification vote);
        public static event VoteReceived OnVoteReceived;

        public delegate void MatchmakingInviteReceived(MQTTMatchInvite invite);
        public static event MatchmakingInviteReceived OnMatchmakingInviteReceived;

        public delegate void JukeboxPlaylistUpdated(MQTTJukeBoxPlaylist playList);
        public static event JukeboxPlaylistUpdated OnJukeboxPlaylistUpdated;

        public delegate void JukeboxSongUpdated(MQTTCurrentSong song);
        public static event JukeboxSongUpdated OnJukeboxSongUpdated;

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
                for (int i = 0; pendingCount > i; i++)
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
                while (true)
                {
                    if (ServerManager.Instance.Clan != null)
                    {
                        Debug.Log($"Clan Found: Starting subscription.");
                        SubscribeToClanNotifications();
                        break;
                    }
                    Debug.LogWarning($"Clan not found: Trying again.");
                    await Task.Delay(1000);
                }
            }
        }

        public async void SubscribeToClanNotifications()
        {
            Task voting = SubscribeToVoting();
            Task dailyTask = SubscribeToDailyTask();
            Task jukeBox = SubscribeToJukebox();
            Task matchmaking = SubscribeToMatchmaking();

            List<Task> tasks = new List<Task> { voting, dailyTask, jukeBox, matchmaking };

            while (tasks.Count > 0)
            {
                Task finishedTask = await Task.WhenAny(tasks);
                await finishedTask;
                tasks.Remove(finishedTask);
            }
            _subscriptionsDone = true;
            OnMQTTConnectionEstablished?.Invoke(true);
        }

        public async void UnsubscribeFromClanNotifications()
        {
            await UnsubscribeFromVoting();
            await UnsubscribeFromDailyTask();
            await UnsubscribeFromJukebox();
            await UnsubscribeFromMatchmaking();
            _subscriptionsDone = false;

        }

        public async Task SubscribeToVoting()
        {
            if (_client == null || !_client.IsConnected || _votingTopic != null) return;

            _votingTopic = VotingTopicBase.Replace("{clanId}", ServerManager.Instance.Clan._id);

            try
            {
                Task task= _client.SubscribeAsync(_votingTopic);
                Debug.Log($"Subscribing to {_votingTopic}");
                while (!task.IsCompleted)
                {
                    Task ongoingTask = await Task.WhenAny(task);
                    await ongoingTask;
                }
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
                Task task = _client.SubscribeAsync(_dailyTaskPlayerTopic);
                Debug.Log($"Subscribing to {_dailyTaskPlayerTopic}");
                Task task2 = _client.SubscribeAsync(_dailyTaskClanTopic);
                Debug.Log($"Subscribing to {_dailyTaskClanTopic}");

                List<Task> tasks = new List<Task> { task, task2 };

                while (tasks.Count > 0)
                {
                    Task finishedTask = await Task.WhenAny(tasks);
                    if(finishedTask == task)
                    {
                        Debug.Log($"Subscribtion to {_dailyTaskPlayerTopic} successful");
                    }
                    else if (finishedTask == task2)
                    {
                        Debug.Log($"Subscribtion to {_dailyTaskClanTopic} successful");
                    }
                    await finishedTask;
                    tasks.Remove(finishedTask);
                }
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
                Task task = _client.SubscribeAsync(_matchmakingInviteTopic);
                Debug.Log($"Subscribing to {_matchmakingInviteTopic}");
                while (!task.IsCompleted)
                {
                    Task ongoingTask = await Task.WhenAny(task);
                    await ongoingTask;
                }
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
                Task task = _client.SubscribeAsync(_jukeboxTopic);
                Debug.Log($"Subscribing to {_jukeboxTopic}");
                while (!task.IsCompleted)
                {
                    Task ongoingTask = await Task.WhenAny(task);
                    await ongoingTask;
                }
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
                if (result["topic"].ToString().Equals("jukebox"))
                {
                    switch (result["type"].ToString())
                    {
                        case "SONG_UPDATED":
                            MQTTCurrentSong song = result["payload"]["song"].ToObject<MQTTCurrentSong>();
                            OnJukeboxSongUpdated?.Invoke(song);
                            break;
                        case "PLAYLIST_UPDATED":
                            MQTTJukeBoxPlaylist playList = result["payload"]["playlist"].ToObject<MQTTJukeBoxPlaylist>();
                            OnJukeboxPlaylistUpdated?.Invoke(playList);
                            break;
                    }
                }
                else if (result["topic"].ToString().Equals("matchmaking"))
                {
                    if (result["type"].ToString().Equals("INVITE_UPDATED"))
                    {
                        MQTTMatchInvite invite = result["payload"].ToObject<MQTTMatchInvite>();

                        OnMatchmakingInviteReceived?.Invoke(invite);
                    }
                }
                else if (result["topic"].ToString().Equals("voting"))
                {
                    switch (result["type"].ToString())
                    {
                        case "VOTING_CREATED":
                            switch (result["payload"]["type"].ToString())
                            {
                                case "flea_market_sell_item":
                                    MQTTFurnitureNotification sellFurniture = result["payload"].ToObject<MQTTFurnitureNotification>();
                                    OnFurnitureSellPollCreatedReceived?.Invoke(sellFurniture);
                                    break;
                                case "flea_market_buy_item":
                                    MQTTFurnitureNotification buyStallFurniture = result["payload"].ToObject<MQTTFurnitureNotification>();
                                    OnFurnitureStallBuyReceived?.Invoke(buyStallFurniture);
                                    break;
                                case "change_item_price":
                                    MQTTFurnitureNotification changeFurniture = result["payload"].ToObject<MQTTFurnitureNotification>();
                                    OnFurnitureSellChangedReceived?.Invoke(changeFurniture);
                                    break;
                                case "shop_buy_item":
                                    MQTTFurnitureNotification buyFurniture = result["payload"].ToObject<MQTTFurnitureNotification>();
                                    OnFurnitureBuyReceived?.Invoke(buyFurniture);
                                    break;
                                case "set_clan_role":
                                    break;
                                case "clan_governance_update":
                                    break;
                            }
                            break;
                        case "VOTING_UPDATED":
                            MQTTVotingUpdatedNotification vote = result["payload"].ToObject<MQTTVotingUpdatedNotification>();
                            OnVoteReceived?.Invoke(vote);
                            break;
                        case "VOTING_ENDED":
                            break;
                        case "VOTING_ERROR":
                            break;
                    }
                    Debug.Log($"Voting received: {message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Payload parsing failed: {ex}");
            }
        }
    }
}
