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


/// <summary>
/// ChatListener is the main class handling interaction bewtween Photon chat service, our own server for chat history and the game.
/// </summary>
/// <remarks>
/// AltZone's chat feature is built using Photon Chat. Photon Chat handles subscribing to chat rooms, sending and receiving messages over internet. Chat history is saved and retrieved
/// from AltZone's own server.
/// </remarks>
public class ChatListener : MonoBehaviour
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

    public enum ServerHistoryLoadingCause
    {
        FirstTimeLoading,
        LoadMoreButtonClicked,
        TimedOutFromPhotonChat
    }

    public static ChatListener Instance { get; private set; }

    internal string _id;
    internal string _username;

    internal bool _chatPreviewIsEnabled;

    private WebSocket _socket;
    private ChatController _chatController;                 // Controller for the main chat
    private ChatPreviewController _chatPreviewController;   // Controller for the small chat preview outside of main chat

    [SerializeField] internal ChatChannel _activeChatChannel, _globalChatChannel, _clanChatChannel, _countryChatChannel;
    [SerializeField] internal ChatChannel[] _chatChannels;

    [SerializeField] internal List<ChatMessage> _chatMessages;

    //private ChatAppSettings _appSettings;

    private const string SERVER_ADDRESS = "https://devapi.altzone.fi/chat/";

    internal ChatPreviewController ChatPreviewController { get => _chatPreviewController; set => _chatPreviewController = value; }
    internal ChatController ChatController { get => _chatController; set => _chatController = value; }
    //internal ChatClient ChatClient { get => _chatClient; set => _chatClient = value; }

    public string AccessToken { get => PlayerPrefs.GetString("accessToken", string.Empty); }

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
        StartCoroutine(ServerManager.Instance.GetMessageHistory("global", success => { }));
        HandleAccountChange(ServerManager.Instance.isLoggedIn);
    }

    private void Update()
    {
        if (_socket != null && _socket.State == WebSocketState.Open) _socket.DispatchMessageQueue();
    }

    private void OnDestroy()
    {
        CloseSocket();
    }

    private async void OpenSocket()
    {
        Dictionary<string, string> header = new Dictionary<string, string> { { "Authorization", $"Bearer {AccessToken}" } };
        Debug.LogWarning(AccessToken);
        Debug.LogWarning("Connecting to chat.");
        if (_socket == null)
        {
            _socket = new("wss://devapi.altzone.fi/latest-release/ws/chat", header);
        }

        _socket.OnOpen += () => { Debug.LogWarning("Connected to chat."); _id = ServerManager.Instance.Player._id; SendMessage("test", Emotion.Sorrow, ChatChannelType.Global); };
        _socket.OnError += (errorMessage) => Debug.LogError($"Error: {errorMessage}");
        _socket.OnClose += (code) => Debug.LogWarning($"Disconnected from chat: {code}");
        _socket.OnMessage += HandleMessage;

        await _socket.Connect();

        Invoke("TestMessage", 1);
    }

    private async void CloseSocket()
    {
        _id = null;
        await _socket.Close();
    }

    private void HandleAccountChange(bool loggedIn)
    {
        if (loggedIn)
        {
            if(_socket != null && (_socket.State == WebSocketState.Open && _id != ServerManager.Instance?.Player._id)) CloseSocket();
            if(_id != ServerManager.Instance.Player._id) OpenSocket();
        }
        else CloseSocket();
    }

    private void HandleMessage(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);
        Debug.LogWarning(json);
    }

    public async void SendMessage(string message, Emotion emotion, ChatChannelType channel)
    {
        if(_socket.State == WebSocketState.Open)
        {
            string EventType = null;
            if (ChatChannelType.Global == channel) EventType = "globalMessage";
            else if (ChatChannelType.Clan == channel) EventType = "clanMessage";
            string body = JObject.FromObject(
            new
            {
                Event = EventType,
                data = new {
                    content = message,
                    feeling = emotion.ToString()
                }
            }

            ).ToString();

            //body = "{ 'event': 'globalMessage','data': { 'content': 'test','feeling': 'Sorrow'}}";

        Debug.LogWarning(body);
        await _socket.SendText(body);
        }
    }

    private void TestMessage()
    {
        SendMessage("test", Emotion.Sorrow, ChatChannelType.Global);
    }
}
