using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChatListener;

public class ChatChannel
{
    internal string _channelName;
    internal ChatChannelType _chatChannelType;
    internal int _lastMsgIndex;

    internal ChatChannel()
    {
        _channelName = String.Empty;
        _chatChannelType = ChatChannelType.None;
        _lastMsgIndex = 0;
    }

    internal ChatChannel(string channelName, ChatChannelType channelType)
    {
        _channelName = channelName;
        _chatChannelType = channelType;
        _lastMsgIndex = 0;
    }
}

public class ChatMessage : IEquatable<ChatMessage>
{
    internal string _id;
    internal string _username;
    internal string _message;
    internal string _channelName;
    internal ChatChannelType _chatChannelType;
    internal Mood _mood;

    internal ChatMessage()
    {
        _id = string.Empty;
        _username = string.Empty;
        _message = string.Empty;
        _channelName = string.Empty;
        _chatChannelType = ChatChannelType.None;
        _mood = Mood.None;
    }

    internal ChatMessage(string id, string name, string message, string channelName)
    {
        _id = id;
        _username = name;
        _message = message;
        _channelName = channelName;
        _chatChannelType = ChatChannelType.None;
        _mood = Mood.None;
    }

    internal ChatMessage(string id, string name, string message, string channelName, Mood mood, ChatChannelType chatChannelType)
    {
        _id = id;
        _username = name;
        _message = message;
        _channelName = channelName;
        _mood = mood;
        _chatChannelType = chatChannelType;
    }

    public bool Equals(ChatMessage other)
    {
        if (other is null)
            return false;

        return _username == other._username && _message == other._message && _channelName == other._channelName && _chatChannelType == other._chatChannelType && _mood == other._mood;
    }

    public override bool Equals(object obj) => Equals(obj as ChatMessage);
    public override int GetHashCode() => (_username, _message, _channelName, _chatChannelType, _mood).GetHashCode();
}


public class ChatListener : MonoBehaviour, IChatClientListener
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

    public static ChatListener Instance { get; private set; }

    internal string _username;
    internal bool _chatPreviewIsEnabled;

    private ChatClient _chatClient;
    private ChatController _chatController;         // Controller for the main chat
    private ChatPreviewController _chatPreviewController;   // Controller for the small chat preview

    internal ChatChannel _activeChatChannel, _globalChatChannel, _clanChatChannel, _countryChatChannel;
    internal ChatChannel[] _chatChannels;

    internal List<ChatMessage> _chatMessages;

    private ChatAppSettings _appSettings;

    internal ChatPreviewController ChatPreviewController { get => _chatPreviewController; set => _chatPreviewController = value; }
    internal ChatController ChatController { get => _chatController; set => _chatController = value; }
    internal ChatClient ChatClient { get => _chatClient; set => _chatClient = value; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        #region AppSettings

        _appSettings = new ChatAppSettings();
        _appSettings.AppIdChat = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat;
        _appSettings.AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
        _appSettings.FixedRegion = "EU";
        _appSettings.Protocol = ConnectionProtocol.Udp;
        _appSettings.EnableProtocolFallback = true;
        _appSettings.NetworkLogging = DebugLevel.ERROR;

        #endregion

        _username = "TestUser-" + UnityEngine.Random.Range(0, 10000);
        _chatPreviewIsEnabled = true;

        _globalChatChannel = new ChatChannel("Global", ChatChannelType.Global);
        _clanChatChannel = new ChatChannel("Clan 1", ChatChannelType.Clan);
        _countryChatChannel = new ChatChannel("Country 1", ChatChannelType.Country);
        _chatChannels = new ChatChannel[] { _globalChatChannel, _clanChatChannel, _countryChatChannel };

        _activeChatChannel = _globalChatChannel;
        _chatMessages = new List<ChatMessage>();
        ChatClient = new ChatClient(this);
    }

    private void Start()
    {
        ChatPreviewController = FindObjectOfType<ChatPreviewController>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConnectToPhotonChat();
    }

    private void Update()
    {
        ChatClient.Service();
    }

    public void ConnectToPhotonChat()
    {
        if (!ChatClient.CanChat)
        {
            if (ChatTester.Instance != null && ChatTester.Instance.isActive)
                ClearChatMessages();

            ChatClient.ConnectUsingSettings(_appSettings);
        }
    }
    private void SubscribeToChannels()
    {
        string[] channelsToSubscribe = { _globalChatChannel._channelName, _clanChatChannel._channelName, _countryChatChannel._channelName };
        ChatClient.Subscribe(channelsToSubscribe, -1);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "10-MenuUi" || scene.name == "20-Lobby")
        {
            if (!ChatClient.CanChat)
                ConnectToPhotonChat();
        }
        else
        {
            if (ChatClient.CanChat)
                ChatClient.Disconnect();
        }
    }

    private void ClearChatMessages()
    {
        _chatMessages.Clear();

        if (ChatTester.Instance != null && ChatTester.Instance.isActive)
        {
            foreach (ChatChannel channel in _chatChannels)
                channel._lastMsgIndex = 0;
        }
    }

    #region IChatClientListener implementation

    public void DebugReturn(DebugLevel level, string message) => Debug.Log($"{level}: {message}");
    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"State changed: {state}");
    }
    public void OnConnected()
    {
        Debug.Log("Connected to chat.");

        SubscribeToChannels();
        ChatPreviewController?.OnActiveChatWindowChange(_activeChatChannel);
    }
    public void OnDisconnected()
    {
        Debug.Log("Disconnected from chat.");

        if (ChatClient.DisconnectedCause != ChatDisconnectCause.DisconnectByClientLogic && ChatClient.DisconnectedCause != ChatDisconnectCause.DisconnectByServerLogic)
            ConnectToPhotonChat();
    }
    public void OnGetMessages(string channelName, string[] senders, object[] data)
    {
        Debug.Log($"New messages from channel {channelName}");

        for (int i = 0; i < senders.Length; i++)
        {
            object[] dataToParse = (object[])data[i];
            int index = Array.FindIndex(_chatChannels, item => item._channelName == channelName); // Index of the matching chat channel from _chatChannels array
            string message = dataToParse[1].ToString();

            //if (ChatTester.Instance != null && ChatTester.Instance.isActive)
            //    message = _chatChannels[index]._lastMsgIndex++ + " - " + dataToParse[1].ToString();
            //else
            //    message = dataToParse[1].ToString();

            ChatMessage chatMessage = new ChatMessage(senders[i], dataToParse[0].ToString(), message, channelName, (Mood)dataToParse[2], (ChatChannelType)dataToParse[3]);
            _chatMessages.Add(chatMessage);

            Debug.Log($"Sender: {senders[i]}, Message: {message}");

            if (channelName == _activeChatChannel._channelName)
                ChatPreviewController?.MessageReceived(_chatChannels[index]);

            ChatController?.InstantiateChatMessagePrefab(chatMessage);
        }
    }
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        Debug.Log($"New private from channel {channelName}");
        Debug.Log($"Sender: {sender}, Message: {message}");
    }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log("Status updated");
        Debug.Log($"User: {user}, Status: {status}, gotMessage; {gotMessage}, message: {message}");
    }
    public void OnSubscribed(string[] channels, bool[] results)
    {
        string debugString = "Subscribed to channels: ";

        for (int i = 0; i < channels.Length; i++)
            debugString += $"[{channels[i]}: {results[i]}] ";

        Debug.Log(debugString);
    }
    public void OnUnsubscribed(string[] channels)
    {
        string debugString = "Unsubscribed from channels: ";

        for (int i = 0; i < channels.Length; i++)
            debugString += ($"[{channels[i]}] ");

        Debug.Log(debugString);
    }
    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log($"A user has subscribed to {channel}: {user}");
    }
    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.Log($"A user has unsubscribed from {channel}: {user}");
    }

    #endregion
}
