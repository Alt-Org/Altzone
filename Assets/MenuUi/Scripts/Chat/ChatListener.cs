using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using ExitGames.Client.Photon;
using Newtonsoft.Json.Linq;
using Photon.Chat;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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
    private ChatPreviewController _chatPreviewController;   // Controller for the small chat preview outside of main chat

    [SerializeField] internal ChatChannel _activeChatChannel, _globalChatChannel, _clanChatChannel, _countryChatChannel;
    [SerializeField] internal ChatChannel[] _chatChannels;

    [SerializeField] internal List<ChatMessage> _chatMessages;

    private ChatAppSettings _appSettings;

    internal string _serverAddress = "https://altzone.fi/api/chat/";

    internal ChatPreviewController ChatPreviewController { get => _chatPreviewController; set => _chatPreviewController = value; }
    internal ChatController ChatController { get => _chatController; set => _chatController = value; }
    internal ChatClient ChatClient { get => _chatClient; set => _chatClient = value; }

    private const string DEFAULT_CLAN_CHAT_NAME = "Klaanittomat";

    private const string ERROR_CREATING_CHAT_TO_SERVER = "Chattia ei pystytty luomaan palvelimelle!";
    private const string ERROR_RETRIEVING_CHAT_FROM_SERVER = "Chattia ei pystytty ladata palvelimelta!";
    private const string ERROR_POSTING_MESSAGE_TO_SERVER = "Viestiä ei pystytty tallentamaan serverille!";

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


        _username = PlayerPrefs.GetString("ChatUsername", "TestUser-" + UnityEngine.Random.Range(0, 10000));
        _id = Guid.NewGuid().ToString();

        _chatPreviewIsEnabled = true;

        _globalChatChannel = new ChatChannel("Globaali", ChatChannelType.Global);
        _clanChatChannel = new ChatChannel(DEFAULT_CLAN_CHAT_NAME, ChatChannelType.Clan);
        _countryChatChannel = new ChatChannel("Maa", ChatChannelType.Country);
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

    private void OnEnable()
    {
        ServerManager.OnLogInStatusChanged += SetPlayerValues;
        ServerManager.OnClanChanged += OnClanChanged;
    }
    private void OnDisable()
    {
        ServerManager.OnLogInStatusChanged -= SetPlayerValues;
        ServerManager.OnClanChanged -= OnClanChanged;
    }

    private void SetPlayerValues(bool isLoggedIn)
    {
        var store = Storefront.Get();
        PlayerData playerData = null;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
        _username = playerData.Name;
    }

    private void OnClanChanged(ServerClan clan)
    {
        StartCoroutine(ChangeClanChat(clan));
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

    internal void DeleteChatHistory(ChatChannel channel)
    {
        channel.ResetIndexes();
        _chatMessages.RemoveAll(item => item._channel._channelName == channel._channelName);
        ChatController?.DeleteChatHistory(channel);

        if (channel == _activeChatChannel)
            ChatPreviewController?.DeleteChatHistory();
    }

    private void DeleteChatDebug(string channelName, string sender, object[] data, ChatChannel channel)
    {
        string username = data[0].ToString();
        string message = data[1].ToString();

        string[] substrings = message.Split("?");

        if (substrings.Length <= 1)
        {
            if (sender == _id)
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
                                data[1] = message;
                                ChatClient.PublishMessage(channel._channelName, data, true);
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

            if (sender != _id)
            {
                StartCoroutine(GetChatHistoryAndSubscribe(new ChatChannel[] { channel }, null));
            }
        }
    }

    public IEnumerator ChangeClanChat(ServerClan clan)
    {
        string clanChatName = string.Empty;

        if (clan == null)
            clanChatName = DEFAULT_CLAN_CHAT_NAME;
        else
            clanChatName = clan.name;

        yield return new WaitUntil(() => ChatClient.CanChatInChannel(_clanChatChannel._channelName));

        ChatClient.Unsubscribe(new string[] { _clanChatChannel._channelName });
        DeleteChatHistory(_clanChatChannel);
        _clanChatChannel._channelName = clanChatName;

        ChatController?.OnClanChatChanged(clanChatName);
        ChatPreviewController?.OnActiveChatWindowChange(_activeChatChannel);
        StartCoroutine(GetChatHistoryAndSubscribe(new ChatChannel[] { _clanChatChannel }, null));
    }

    #region Server Webrequests
    internal void GetChatHistoryFromServer(ChatChannel channel, ServerHistoryCause serverHistoryCause, Action<bool> callbackOnFinish) { StartCoroutine(GetChatHistoryFromServerCoroutine(channel, serverHistoryCause, callbackOnFinish)); }

    private IEnumerator PostChatToServerCoroutine(string channelName, System.Action<JObject> callbackOnFinish)
    {
        string json = "{\"name\": \"" + channelName + "\"}";
        var bytes = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = UnityWebRequest.Put(_serverAddress, bytes))
        {
            request.method = "POST"; // Hack to send POST to server instead of PUT
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ChatController?.ShowErrorMessage(ERROR_CREATING_CHAT_TO_SERVER + "\n" + request.error);

                if (callbackOnFinish != null)
                    callbackOnFinish(null);
            }
            else
            {
                if (callbackOnFinish != null)
                    callbackOnFinish(JObject.Parse(request.downloadHandler.text));
            }
        }
    }

    private IEnumerator GetChatFromServerCoroutine(string channelName, System.Action<JObject> callbackOnFinish)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(_serverAddress + @"?search=name=""" + channelName + @""""))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ChatController?.ShowErrorMessage(ERROR_RETRIEVING_CHAT_FROM_SERVER + " - " + channelName + "\n" + request.error);

                if (callbackOnFinish != null)
                    callbackOnFinish(null);
            }
            else
            {
                JObject json = JObject.Parse(request.downloadHandler.text);
                JArray chats = (JArray)json["data"]["Chat"];

                if (callbackOnFinish != null)
                    callbackOnFinish((JObject)chats.Where(name => (string)name["name"] == channelName).FirstOrDefault());
            }
        }
    }

    private IEnumerator GetChatHistoryFromServerCoroutine(ChatChannel channel, ServerHistoryCause serverHistoryCause, System.Action<bool> callbackOnFinish)
    {
        string query = string.Empty;

        if (serverHistoryCause == ServerHistoryCause.FirstTimeLoading || serverHistoryCause == ServerHistoryCause.TimedOutFromPhotonChat)
            query = $"{_serverAddress}{channel._id}/messages?sort=id&desc&limit=50";
        else if (serverHistoryCause == ServerHistoryCause.LoadMoreButtonClicked)
            query = $"{_serverAddress}{channel._id}/messages?search=id<{channel._firstMsgIndex};AND;id>{channel._firstMsgIndex - 50}&sort=id&asc&limit=50";

        using (UnityWebRequest request = UnityWebRequest.Get(query))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                //ChatController?.ShowErrorMessage(ERROR_RETRIEVING_CHAT_FROM_SERVER + " - " + channel._channelName + "\n" + request.error);

                if (callbackOnFinish != null)
                    callbackOnFinish(false);
            }
            else
            {
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
    }
    private IEnumerator PostMessageToServerCoroutine(ChatMessage message, ChatMessagePrefab chatMessageInstance)
    {
        if (message._message == null || message._message.Length == 0)
            yield break;

        int enumIndex = (int)message._mood;
        string json = "{\"id\":" + message._id + ",\"senderUsername\":\"" + message._username + "\",\"content\":\"" + message._message + "\",\"feeling\":" + enumIndex.ToString() + "}";
        var bytes = Encoding.UTF8.GetBytes(json);

        using(UnityWebRequest request = UnityWebRequest.Put($"{_serverAddress}{message._channel._id}/messages", bytes))
        {
            request.method = "POST"; // Hack to send POST to server instead of PUT
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (chatMessageInstance != null)
                {
                    chatMessageInstance.SetErrorColor();
                }
                ChatController?.ShowErrorMessage(ERROR_POSTING_MESSAGE_TO_SERVER + "\n" + request.error);
            }
            else
                Debug.Log("Message successfully uploaded to server!");
        }
    }

    private IEnumerator DeleteChat(string id, Action<bool> callbackOnFinish)
    {
        using(UnityWebRequest request = UnityWebRequest.Delete(_serverAddress + id))
        {
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
                            Debug.LogWarning("Could not get chat history for " + channel._channelName + ". This chat might not any messages posted yet.");
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
                DeleteChatDebug(channelName, senders[i], dataToParse, channel);
            }
            else
            {
                ChatMessage chatMessage = new ChatMessage(id, username, message, channel, (Mood)dataToParse[2]);
                _chatMessages.Add(chatMessage);

                if (channelName == _activeChatChannel._channelName)
                    ChatPreviewController?.MessageReceived(channel);

                ChatMessagePrefab chatMessageInstance = ChatController?.InstantiateChatMessagePrefab(chatMessage, false);

                if (senders[i] == _id)
                {
                    StartCoroutine(PostMessageToServerCoroutine(chatMessage, chatMessageInstance));
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
