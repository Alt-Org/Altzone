using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Altzone.Scripts;
using Altzone.Scripts.Common;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using NativeWebSocket;
//using ExitGames.Client.Photon;
using Newtonsoft.Json.Linq;
//using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Altzone.Scripts.Chat
{
    public enum ChatChannelType
    {
        Global,
        Clan,
        Country,
        None
    }

    public enum Mood
    {
        Happy,
        Sad,
        Neutral,
        Love,
        Thinking,
        Wink,
        Angry,
        None
    }
    /// <summary>
    /// ChatListener is the main class handling interaction bewtween Photon chat service, our own server for chat history and the game.
    /// </summary>
    /// <remarks>
    /// AltZone's chat feature is built using Photon Chat. Photon Chat handles subscribing to chat rooms, sending and receiving messages over internet. Chat history is saved and retrieved
    /// from AltZone's own server.
    /// </remarks>
    public class ChatListener : MonoBehaviour
    {
        public enum ServerHistoryLoadingCause
        {
            FirstTimeLoading,
            LoadMoreButtonClicked,
            TimedOutFromPhotonChat
        }

        public static ChatListener Instance { get; private set; }

        internal string _id;
        private string _username;

        private bool chatPreviewIsEnabled = true;

        private WebSocket _socket;
        //private ChatController _chatController;                 // Controller for the main chat
        //private ChatPreviewController _chatPreviewController;   // Controller for the small chat preview outside of main chat

        private ChatChannelType _activeChatChannel;
        private ChatChannel _globalChatChannel, _clanChatChannel;
        private ChatChannel[] _chatChannels;

        [SerializeField] private List<ChatMessage> _chatMessages;

        //private ChatAppSettings _appSettings;

        private const string SERVER_ADDRESS = "https://devapi.altzone.fi/chat/";

        //internal ChatPreviewController ChatPreviewController { get => _chatPreviewController; set => _chatPreviewController = value; }
        //internal ChatController ChatController { get => _chatController; set => _chatController = value; }
        //internal ChatClient ChatClient { get => _chatClient; set => _chatClient = value; }

        private bool _globalChatFetched = false;
        private bool _clanChatFetched = false;

        public delegate void ActiveChannelChanged(ChatChannelType chatChannelType);
        public static event ActiveChannelChanged OnActiveChannelChanged;

        private Coroutine _socketPolling;

        public string AccessToken { get => PlayerPrefs.GetString("accessToken", string.Empty); }
        public bool ChatPreviewIsEnabled { get => chatPreviewIsEnabled; set => chatPreviewIsEnabled = value; }
        public List<ChatMessage> ChatMessages { get
            {
                return _activeChatChannel switch
                {
                    ChatChannelType.Global => _globalChatChannel.ChatMessages,
                    ChatChannelType.Clan => _clanChatChannel.ChatMessages,
                    _ => null,
                };
            }
        }
        public string Username { get => _username; set => _username = value; }
        public ChatChannelType ActiveChatChannel { get => _activeChatChannel; set { _activeChatChannel = value; OnActiveChannelChanged?.Invoke(_activeChatChannel); } }

        public ChatChannel GetActiveChannel
        {
            get
            {
                return _activeChatChannel switch
                {
                    ChatChannelType.Global => _globalChatChannel,
                    ChatChannelType.Clan => _clanChatChannel,
                    _ => null,
                };
            }
        }

        public bool GlobalChatFetched { get => _globalChatFetched;}
        public bool ClanChatFetched { get => _clanChatFetched;}
        public ChatChannel[] ChatChannels { get => _chatChannels;}

        private const string DEFAULT_CLAN_CHAT_NAME = "Klaanittomat";

        private const string ERROR_CREATING_CHAT_TO_SERVER = "Chattia ei pystytty luomaan palvelimelle!";
        private const string ERROR_RETRIEVING_CHAT_FROM_SERVER = "Chattia ei pystytty ladata palvelimelta!";
        private const string ERROR_POSTING_MESSAGE_TO_SERVER = "Viestiä ei pystytty tallentamaan serverille!";

        public delegate void MessageReceived(bool isLoggedIn);
        public static event MessageReceived OnMessageReceived;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            ServerManager.OnLogInStatusChanged += HandleAccountChange;
            HandleAccountChange(ServerManager.Instance.isLoggedIn);
        }

        private void OnDestroy()
        {
            ServerManager.OnLogInStatusChanged -= HandleAccountChange;
            CloseSocket();
        }

        private IEnumerator InitializeChats()
        {
            _globalChatFetched = false;
            _clanChatFetched = false;
            StartCoroutine(ServerManager.Instance.GetMessageHistory(ChatChannelType.Global, success =>
            {
                _globalChatChannel = new("Global", ChatChannelType.Global);
                if (success != null)
                {
                    List<ChatMessage> messageList = new();
                    foreach (ServerChatMessage message in success)
                    {
                        messageList.Insert(0,new(message));
                    }
                    _globalChatChannel.SetChatHistory(messageList);
                }
                _globalChatFetched = true;
            }));

            yield return new WaitUntil(() => GlobalChatFetched);

            StartCoroutine(ServerManager.Instance.GetMessageHistory(ChatChannelType.Clan, success =>
            {
                _clanChatChannel = new("Clan", ChatChannelType.Clan);
                if (success != null)
                {
                    List<ChatMessage> messageList = new();
                    foreach (ServerChatMessage message in success)
                    {
                        messageList.Insert(0, new(message));
                    }
                    _clanChatChannel.SetChatHistory(messageList);
                }
                _clanChatFetched = true;
            }));

            yield return new WaitUntil(() => ClanChatFetched);

            yield return new WaitUntil(() => _socket != null && _socket.State == WebSocketState.Open);
#if !UNITY_WEBGL
            _socketPolling = StartCoroutine(PollWebSocket());
#endif
        }

        private async void OpenSocket()
        {
            Dictionary<string, string> header = new Dictionary<string, string> { { "Authorization", $"Bearer {AccessToken}" } };
            Debug.Log("Connecting to chat.");
            if (_socket == null)
            {
                _socket = new("wss://devapi.altzone.fi/ws/chat", header);
            }

            _socket.OnOpen += () => { Debug.Log("Connected to chat."); _id = ServerManager.Instance.Player._id; TestMessage(); };
            _socket.OnError += (errorMessage) => Debug.LogError($"Error: {errorMessage}");
            _socket.OnClose += (code) => Debug.LogWarning($"Disconnected from chat: {code}");
            _socket.OnMessage += HandleMessage;

            await _socket.Connect();
        }

        private async void CloseSocket()
        {
            _id = null;
            if (_socket != null)
                await _socket.Close();
            if(_socketPolling != null)StopCoroutine(_socketPolling);
            _socket = null;
        }

        private void HandleAccountChange(bool loggedIn)
        {
            if (loggedIn)
            {
                StartCoroutine(InitializeChats());
                if (_socket != null && (_socket.State == WebSocketState.Open && _id != ServerManager.Instance?.Player._id)) CloseSocket();
                if (_id != ServerManager.Instance.Player._id) OpenSocket();
            }
            else CloseSocket();
        }
#if !UNITY_WEBGL
        private IEnumerator PollWebSocket()
        {
            while (true)
            {
                if (_socket != null && _socket.State == WebSocketState.Open) _socket.DispatchMessageQueue();
                else break;
                yield return null;
            }
            _socketPolling = null;
        }
#endif

        private void HandleMessage(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            Debug.LogWarning(JObject.Parse(json));
            JToken middleresult = JObject.Parse(json)["message"];
            ServerChatMessage message = middleresult["message"].ToObject<ServerChatMessage>();
            if (middleresult["event"].ToString().Equals("newMessage"))
            {
                if (middleresult["chat"].ToString().Equals("clan")) _clanChatChannel.AddNewMessage(new(message));
                else if (middleresult["chat"].ToString().Equals("global")) _globalChatChannel.AddNewMessage(new(message));
            }
            else if (middleresult["event"].ToString().Equals("newReaction"))
            {
                if (middleresult["chat"].ToString().Equals("clan")) _clanChatChannel.UpdateReactions(message._id, message.reactions);
                else if (middleresult["chat"].ToString().Equals("global")) _globalChatChannel.UpdateReactions(message._id, message.reactions);
            }
        }

        public async void SendMessage(string message, Mood emotion, ChatChannelType channel)
        {
            if (_socket.State == WebSocketState.Open)
            {
                string EventType = null;
                if (ChatChannelType.Global == channel) EventType = "globalMessage";
                else if (ChatChannelType.Clan == channel) EventType = "clanMessage";
                string body = JObject.FromObject(
                new
                {
                    @event = EventType,
                    data = new
                    {
                        content = message,
                        feeling = emotion.ToString()
                    }
                }).ToString();

                await _socket.SendText(body);
            }
        }

        public async void SendReaction(string message, string id, ChatChannelType channel)
        {
            if (_socket.State == WebSocketState.Open)
            {
                string EventType = null;
                if (ChatChannelType.Global == channel) EventType = "globalMessageReaction";
                else if (ChatChannelType.Clan == channel) EventType = "clanMessageReaction";
                string body = JObject.FromObject(
                new
                {
                    @event = EventType,
                    data = new
                    {
                        message_id = id,
                        emoji = message
                    }
                }).ToString();

                await _socket.SendText(body);
            }
        }

        private void TestMessage()
        {
            //SendMessage("test", Mood.Sad, ChatChannelType.Global);
        }

        public ChatChannel GetChatChannel(ChatChannelType type)
        {
            switch (type)
            {
                case ChatChannelType.Global:
                    return _globalChatChannel;
                case ChatChannelType.Clan:
                    return _clanChatChannel;
                default:
                    return null;
            }
        }

        public ChatChannel GetChatChannel(string type)
        {
            switch (type)
            {
                case "global":
                    return _globalChatChannel;
                case "clan":
                    return _clanChatChannel;
                default:
                    return null;
            }
        }
    }
}
