using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using Newtonsoft.Json.Linq;
using Photon.Chat;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static ChatListener;

[Serializable]
public class ChatChannel
{
    [SerializeField] internal string _id;
    [SerializeField] internal string _channelName;
    [SerializeField] internal ChatChannelType _chatChannelType;
    [SerializeField] internal int _firstMsgIndex;
    [SerializeField] internal int _lastMsgIndex;

    internal ChatChannel()
    {
        _id = string.Empty;
        _channelName = string.Empty;
        _chatChannelType = ChatChannelType.None;
        _lastMsgIndex = 0;
        _firstMsgIndex = 1;
    }

    internal ChatChannel(string channelName, ChatChannelType channelType)
    {
        _id = string.Empty;
        _channelName = channelName;
        _chatChannelType = channelType;
        _lastMsgIndex = 0;
        _firstMsgIndex = 0;
    }
    internal ChatChannel(string id, string channelName, ChatChannelType channelType)
    {
        _id = id;
        _channelName = channelName;
        _chatChannelType = channelType;
        _lastMsgIndex = 0;
        _firstMsgIndex = 0;
    }

    internal void Reset()
    {
        _id = string.Empty;
        _channelName = string.Empty;
        _lastMsgIndex = 0;
        _firstMsgIndex = 0;
    }

    internal void ResetIndexes()
    {
        _lastMsgIndex = 0;
        _firstMsgIndex = 0;
    }
}

[Serializable]
public class ChatMessage : IEquatable<ChatMessage>
{
    [SerializeField] internal int _id;
    [SerializeField] internal string _username;
    [SerializeField] internal string _message;
    [SerializeField] internal ChatChannel _channel;
    [SerializeField] internal Mood _mood;

    internal ChatMessage()
    {
        _id = -1;
        _username = string.Empty;
        _message = string.Empty;
        _channel = new ChatChannel();
        _mood = Mood.None;
    }

    internal ChatMessage(int id, string name, string message, ChatChannel channel, Mood mood)
    {
        _id = id;
        _username = name;
        _message = message;
        _channel = channel;
        _mood = mood;
    }

    public bool Equals(ChatMessage other)
    {
        if (other is null)
            return false;

        return _username == other._username && _message == other._message && _channel._channelName == other._channel._channelName && _channel._chatChannelType == other._channel._chatChannelType && _mood == other._mood;
    }

    public override bool Equals(object obj) => Equals(obj as ChatMessage);
    public override int GetHashCode() => (_username, _message, _channel._channelName, _channel._chatChannelType, _mood).GetHashCode();
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

    public enum ServerHistoryCause
    {
        FirstTimeLoading,
        LoadMoreButtonClicked,
        TimedOutFromPhotonChat
    }

    public static ChatListener Instance { get; private set; }

    internal string _id;
    internal string _username;

    internal bool _chatPreviewIsEnabled;

    private ChatClient _chatClient;
    private ChatController _chatController;         // Controller for the main chat
    private ChatPreviewController _chatPreviewController;   // Controller for the small chat preview

    [SerializeField] internal ChatChannel _activeChatChannel, _globalChatChannel, _clanChatChannel, _countryChatChannel;
    [SerializeField] internal ChatChannel[] _chatChannels;

    [SerializeField] internal List<ChatMessage> _chatMessages;

    private ChatAppSettings _appSettings;

    internal string _serverAddress = "https://altzone.fi/chat/";

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

        if (PlayerPrefs.GetString("ChatUsername", string.Empty) != string.Empty)
        {
            _username = PlayerPrefs.GetString("ChatUsername");
        }
        else
        {
            _username = "TestUser-" + UnityEngine.Random.Range(0, 10000);
            PlayerPrefs.SetString("ChatUsername", _username);
        }

        _id = Guid.NewGuid().ToString();

        _chatPreviewIsEnabled = true;

        _globalChatChannel = new ChatChannel("Global", ChatChannelType.Global);
        _clanChatChannel = new ChatChannel("Clan 1", ChatChannelType.Clan);
        _countryChatChannel = new ChatChannel("Country 1", ChatChannelType.Country);
        _chatChannels = new ChatChannel[] { _globalChatChannel, _clanChatChannel, _countryChatChannel };

        _activeChatChannel = _globalChatChannel;
        _chatMessages = new List<ChatMessage>();

        ChatClient = new ChatClient(this);
        ChatClient.AuthValues = new AuthenticationValues();
        ChatClient.AuthValues.UserId = _id;
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
            ChatClient.ConnectUsingSettings(_appSettings);
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

    public void DeleteChatHistory(ChatChannel channel)
    {
        channel.ResetIndexes();
        ChatListener.Instance._chatMessages.RemoveAll(item => item._channel._channelName == channel._channelName);
        ChatController?.DeleteChatHistory(channel);

        if (channel == _activeChatChannel)
            ChatPreviewController?.DeleteChatHistory();
    }

    #region Server
    internal void GetChatHistoryFromServer(ChatChannel channel, ServerHistoryCause serverHistoryCause, Action<bool> callbackOnFinish) { StartCoroutine(GetChatHistoryFromServerCoroutine(channel, serverHistoryCause, callbackOnFinish)); }

    private IEnumerator PostChatToServerCoroutine(string channelName, System.Action<JObject> callbackOnFinish)
    {
        string json = "{\"name\": \"" + channelName + "\"}";

        var bytes = Encoding.UTF8.GetBytes(json);
        var request = UnityWebRequest.Put(_serverAddress, bytes);
        request.method = "POST"; // Hack to send POST to server instead of PUT
        request.SetRequestHeader("Content-Type", "application/json");
        try
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ChatController?.ShowErrorMessage(true, "Could not get chats from server: " + request.error);

                if (callbackOnFinish != null)
                    callbackOnFinish(null);
            }
            else
            {
                ChatController?.ShowErrorMessage(false, null);

                if (callbackOnFinish != null)
                    callbackOnFinish(JObject.Parse(request.downloadHandler.text));
            }
        }
        finally
        {
            request.Dispose();
        }
    }

    private IEnumerator GetAllChatsFromServer(System.Action<JArray> callbackOnFinish)
    {
        UnityWebRequest request = UnityWebRequest.Get(_serverAddress);

        try
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ChatController?.ShowErrorMessage(true, "Could not get chats from server." + request.error);

                if (callbackOnFinish != null)
                    callbackOnFinish(null);
            }
            else
            {
                ChatController?.ShowErrorMessage(false, null);

                JObject json = JObject.Parse(request.downloadHandler.text);
                JArray chats = (JArray)json["data"]["Chat"];

                if (callbackOnFinish != null)
                    callbackOnFinish(chats);
            }
        }
        finally
        {
            request.Dispose();
        }
    }

    private IEnumerator GetChatFromServerCoroutine(string channelName, System.Action<JObject> callbackOnFinish)
    {
        UnityWebRequest request = UnityWebRequest.Get(_serverAddress);

        try
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ChatController?.ShowErrorMessage(true, "Could not get chat from server (" + channelName + "): " + request.error);

                if (callbackOnFinish != null)
                    callbackOnFinish(null);
            }
            else
            {
                ChatController?.ShowErrorMessage(false, null);

                JObject json = JObject.Parse(request.downloadHandler.text);
                JArray chats = (JArray)json["data"]["Chat"];

                if (callbackOnFinish != null)
                    callbackOnFinish((JObject)chats.Where(name => (string)name["name"] == channelName).FirstOrDefault());
            }
        }
        finally
        {
            request.Dispose();
        }
    }

    private IEnumerator GetChatHistoryFromServerCoroutine(ChatChannel channel, ServerHistoryCause serverHistoryCause, System.Action<bool> callbackOnFinish)
    {
        UnityWebRequest request = new UnityWebRequest();

        if (serverHistoryCause == ServerHistoryCause.FirstTimeLoading || serverHistoryCause == ServerHistoryCause.TimedOutFromPhotonChat)
            request = UnityWebRequest.Get($"{_serverAddress}{channel._id}/messages?sort=id&desc&limit=50");
        else if (serverHistoryCause == ServerHistoryCause.LoadMoreButtonClicked)
            request = UnityWebRequest.Get($"{_serverAddress}{channel._id}/messages?search=id<{channel._firstMsgIndex};AND;id>{channel._firstMsgIndex - 50}&sort=id&asc&limit=50");

        yield return request.SendWebRequest();

        try
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                ChatController?.ShowErrorMessage(true, "Could not get chat history from server (" + channel._channelName + "): " + request.error);

                if (callbackOnFinish != null)
                    callbackOnFinish(false);
            }
            else
            {
                ChatController?.ShowErrorMessage(false, null);

                JObject json = JObject.Parse(request.downloadHandler.text);
                JArray messages = (JArray)json["data"]["Chat"];

                for (int i = messages.Count - 1; i >= 0; i--)
                {
                    int id = int.Parse(messages[i]["id"].ToString());
                    string username = messages[i]["senderUsername"].ToString();
                    string message = messages[i]["content"].ToString();
                    Enum.TryParse(messages[i]["feeling"].ToString(), out Mood mood);

                    ChatMessage chatMessage = new ChatMessage(id, username, message, channel, mood);
                    _chatMessages.Add(chatMessage);

                    int index = int.Parse(messages[i]["id"].ToString());

                    if (channel._firstMsgIndex == 0 || index < channel._firstMsgIndex)
                        channel._firstMsgIndex = index;

                    if (channel._lastMsgIndex == 0 || index > channel._lastMsgIndex)
                        channel._lastMsgIndex = index;


                    if (channel == _activeChatChannel)
                        ChatPreviewController?.MessageReceived(_activeChatChannel);

                    if (serverHistoryCause == ServerHistoryCause.LoadMoreButtonClicked)
                        ChatController?.InstantiateChatMessagePrefab(chatMessage, true);
                    else if (serverHistoryCause == ServerHistoryCause.FirstTimeLoading || serverHistoryCause == ServerHistoryCause.TimedOutFromPhotonChat)
                        ChatController?.InstantiateChatMessagePrefab(chatMessage, false);
                }

                if (callbackOnFinish != null)
                    callbackOnFinish(true);
            }
        }
        finally
        {
            request.Dispose();
        }
    }
    private IEnumerator PostMessageToServerCoroutine(ChatMessage message)
    {
        if (message._message == null || message._message.Length == 0)
            yield break;

        int enumIndex = (int)message._mood;
        string json = "{\"id\":" + message._id + ",\"senderUsername\":\"" + message._username + "\",\"content\":\"" + message._message + "\",\"feeling\":" + enumIndex.ToString() + "}";

        var bytes = Encoding.UTF8.GetBytes(json);
        var request = UnityWebRequest.Put($"{_serverAddress}{message._channel._id}/messages", bytes);
        request.method = "POST"; // Hack to send POST to server instead of PUT
        request.SetRequestHeader("Content-Type", "application/json");

        try
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                ChatController?.ShowErrorMessage(true, "Could not post message to server: " + request.error);
            else
                Debug.Log("Message successfully uploaded to server!");
        }
        finally
        {
            request.Dispose();
        }
    }

    private IEnumerator DeleteChat(string id, Action<bool> callbackOnFinish)
    {
        UnityWebRequest request = UnityWebRequest.Delete(_serverAddress + id);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);

            if (callbackOnFinish != null)
                callbackOnFinish(false);
        }
        else
        {
            Debug.Log("Chat Deleted!");

            if (callbackOnFinish != null)
                callbackOnFinish(true);
        }
    }

    private IEnumerator DeleteAllChats()
    {
        StartCoroutine(GetAllChatsFromServer((returnData) =>
        {
            if (returnData == null)
            {
                Debug.LogWarning("GetAllChatsFromServer returned null");
            }
            else
            {
                for (int i = 0; i < returnData.Count; i++)
                {
                    StartCoroutine(DeleteChat(returnData[i]["_id"].ToString(), (success) =>
                    {
                        if (success)
                            Debug.Log("Chat history deleted");
                        else
                            Debug.LogWarning("Could not delete chat history");
                    }));
                }
            }
        }));

        yield break;
    }

    public IEnumerator GetChatHistoryAndSubscribe(ChatChannel[] channels, Action<bool> callbackOnFinish)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            ChatChannel channel = channels[i];

            StartCoroutine(GetChatFromServerCoroutine(channel._channelName, (returnData) =>
            {
                if (returnData == null)
                {
                    StartCoroutine(PostChatToServerCoroutine(channel._channelName, (returnData) =>
                    {
                        if (returnData == null)
                        {
                            Debug.LogWarning("PostChatToServerCoroutine returned null");

                            if (callbackOnFinish != null)
                                callbackOnFinish(false);
                        }
                        else
                        {
                            Debug.Log("Successfully created " + channel._channelName + " to server with id " + returnData["data"]["Chat"]["_id"].ToString());
                            channel._id = returnData["data"]["Chat"]["_id"].ToString();
                            ChatClient.Subscribe(channel._channelName, 0, 0);

                            if (callbackOnFinish != null)
                                callbackOnFinish(true);
                        }
                    }));
                }
                else
                {
                    Debug.Log("Found " + returnData["name"] + " from server. Attempting to get chat history");
                    channel._id = returnData["_id"].ToString();
                    StartCoroutine(GetChatHistoryFromServerCoroutine(channel, ServerHistoryCause.FirstTimeLoading, (returnData) =>
                    {
                        if (returnData == false)
                            Debug.LogWarning("Could not get chat history for " + channel._channelName);
                        else
                            Debug.Log("Chat history found for " + channel._channelName);

                        ChatClient.Subscribe(channel._channelName, 0, 0);

                        if (callbackOnFinish != null)
                            callbackOnFinish(true);
                    }));

                }
            }));
        }
        yield break;
    }


    #endregion

    #region IChatClientListener implementation

    public void DebugReturn(DebugLevel level, string message) => Debug.Log($"{level}: {message}");
    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"State changed: {state}");
    }
    public void OnConnected()
    {
        Debug.Log("Connected to chat");
        //StartCoroutine(DeleteAllChats());
        StartCoroutine(GetChatHistoryAndSubscribe(_chatChannels, null));
    }
    public void OnDisconnected()
    {
        Debug.Log("Disconnected from chat.");

        StopAllCoroutines();

        foreach (ChatChannel channel in _chatChannels)
        {
            DeleteChatHistory(channel);
        }

        if (ChatClient.DisconnectedCause != ChatDisconnectCause.DisconnectByClientLogic && ChatClient.DisconnectedCause != ChatDisconnectCause.DisconnectByServerLogic)
            ConnectToPhotonChat();
    }
    public void OnGetMessages(string channelName, string[] senders, object[] data)
    {
        Debug.Log($"New messages from channel {channelName}");

        for (int i = 0; i < senders.Length; i++)
        {
            object[] dataToParse = (object[])data[i];
            ChatChannel channel = Array.Find(_chatChannels, item => item._channelName == channelName);

            int id = ++channel._lastMsgIndex;
            string username = dataToParse[0].ToString();
            string message = dataToParse[1].ToString();

            if (message.StartsWith("/delete"))
            {
                string[] substrings = message.Split("?");

                if (substrings.Length <= 1)
                {
                    if (senders[i] == _id)
                    {
                        StopAllCoroutines();

                        StartCoroutine(DeleteChat(channel._id, (success) =>
                        {
                            if (success)
                            {
                                StartCoroutine(GetChatHistoryAndSubscribe(new ChatChannel[] { channel }, (success) =>
                                {
                                    if (success)
                                    {
                                        string message = "/delete?resubscribe";
                                        dataToParse[1] = message;
                                        ChatClient.PublishMessage(channel._channelName, dataToParse, true);
                                    }

                                }));
                            }
                            else
                            {
                                Debug.LogWarning("Could not delete chat history");
                            }
                        }));
                    }
                }
                else if (substrings[1] == "resubscribe")
                {
                    DeleteChatHistory(channel);

                    if (senders[i] != _id)
                    {
                        StartCoroutine(GetChatHistoryAndSubscribe(new ChatChannel[] { channel }, null));
                    }
                }
            }
            else
            {
                ChatMessage chatMessage = new ChatMessage(id, username, message, channel, (Mood)dataToParse[2]);
                _chatMessages.Add(chatMessage);

                if (channelName == _activeChatChannel._channelName)
                    ChatPreviewController?.MessageReceived(channel);

                ChatController?.InstantiateChatMessagePrefab(chatMessage, false);

                if (senders[i] == _id)
                {
                    StartCoroutine(PostMessageToServerCoroutine(chatMessage));
                }

            }
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
        {
            debugString += $"[{channels[i]}: {results[i]}] ";
        }

        Debug.Log(debugString);
    }

    public void OnUnsubscribed(string[] channels)
    {
        string debugString = "Unsubscribed from channels: ";

        for (int i = 0; i < channels.Length; i++)
        {
            ChatChannel channel = Array.Find(_chatChannels, item => item._channelName == channels[i]);
            debugString += ($"[{channels[i]}] ");
        }

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
