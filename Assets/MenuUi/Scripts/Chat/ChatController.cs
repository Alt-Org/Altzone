using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Altzone.Scripts.Chat;

/// <summary>
/// ChatController handles sending new chat messages and displaying received chat messages on the Global, Country and Clan chat windows.
/// /// </summary>
public class ChatController : MonoBehaviour
{
    private ChatWindow _activeChatWindow;

    private const string DisconnectedText = "Connection status: <color=red>Disconnected</color>";
    private const string ConnectedText = "Connection status: <color=green>Connected</color>";

    [Header("Appearance")]
    [SerializeField] private int _fontSize;

    [Header("Top Bar")]
    [SerializeField] private TMP_Text _connectionStatusText;
    [SerializeField] private TMP_Text _chatroomNameText;

    [Header("Chat Windows")]
    [SerializeField] private ChatWindow _globalChatWindow;
    [SerializeField] private ChatWindow _clanChatWindow;
    [SerializeField] private ChatWindow _countryChatWindow;
    private ChatWindow[] _chatWindows;

    [Header("Bottom Bar")]
    [SerializeField] private TMP_InputField _chatMessageInputField;
    [SerializeField] private TMP_Dropdown _moodDropdown;
    [SerializeField] private Button _sendMessageButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject _chatMessageLocalPrefab;
    [SerializeField] private GameObject _chatMessageOthersPrefab;

    [Header("Error Handling")]
    [SerializeField] internal GameObject _errorPanel;
    [SerializeField] internal GameObject _errorPrefab;

    public ChatWindow ActiveChatWindow { get => _activeChatWindow; set => _activeChatWindow = value; }

    private void Awake()
    {
        #region Chat Window initialization

        _chatWindows = new ChatWindow[] { _globalChatWindow, _clanChatWindow, _countryChatWindow };
        _globalChatWindow.ChannelType = ChatChannelType.Global;
        _clanChatWindow.ChannelType = ChatChannelType.Clan;
        _countryChatWindow.ChannelType = ChatChannelType.Country;

        _globalChatWindow.ChangeToWindowButton.onClick.AddListener(() => OnChatWindowChanged(ChatChannelType.Global));
        _clanChatWindow.ChangeToWindowButton.onClick.AddListener(() => OnChatWindowChanged(ChatChannelType.Clan));
        _countryChatWindow.ChangeToWindowButton.onClick.AddListener(() => OnChatWindowChanged(ChatChannelType.Country));

        #endregion

        _sendMessageButton.onClick.AddListener(() => SendChatMessage(_chatMessageInputField.text));
    }

    private void Start()
    {
        //ChatListener.Instance.ChatController = this;

        OnChatWindowChanged(ChatListener.Instance.ActiveChatChannel);

        // If we have received chat messages before opening chat for the first time, display those messages.
        if (ChatListener.Instance.ChatMessages != null)
        {
            foreach (var message in ChatListener.Instance.ChatMessages)
                InstantiateChatMessagePrefab(message, false);
        }
    }

    private void Update()
    {
    }

    /// <summary>
    /// Sends a chat message through Photon Chat.
    /// </summary>
    /// <param name="message">Text from InputField</param>
    internal void SendChatMessage(string message)
    {
        if (message == "")
            return;

        object[] dataToSend = new object[] {_chatMessageInputField.text, _moodDropdown.value, ChatListener.Instance.ActiveChatChannel };
        _chatMessageInputField.text = null;
    }

    /// <summary>
    /// Creates an instance from received chat message.
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="instantiateOnTop">Bool representing if the created instance should be instantiated on top of current messages. Older messages are instantiated on top.</param>
    /// <returns></returns>
    internal ChatMessagePrefab InstantiateChatMessagePrefab(ChatMessage message, bool instantiateOnTop)
    {
        ChatWindow chatWindow;

        if (!(chatWindow = Array.Find(_chatWindows, item => item.ChannelType == message.Channel.ChatChannelType)))
            return null;

        ChatMessagePrefab chatMessageInstance;

        // Own messages are on right, other player messages on left
        if (message.Username == ChatListener.Instance.Username)
            chatMessageInstance = Instantiate(_chatMessageLocalPrefab, chatWindow.RectTransform).GetComponent<ChatMessagePrefab>();
        else
            chatMessageInstance = Instantiate(_chatMessageOthersPrefab, chatWindow.RectTransform).GetComponent<ChatMessagePrefab>();

        chatMessageInstance.SetName(message.Username);
        chatMessageInstance.SetMessage(message.Username);
        //chatMessageInstance.SetMessage(message._id + " - " + message._message);
        chatMessageInstance.SetMood(message.Mood);
        //chatMessageInstance.SetProfilePicture(message.Channel.ChatChannelType);
        chatMessageInstance.SetFontSize(_fontSize);

        if (instantiateOnTop)
            chatMessageInstance.gameObject.transform.SetSiblingIndex(1);

        // Check if message id is 1 meaning that the message is first message in chat. If not, there are older messages on server so we set 'ToggleLoadMoreButton' active.
        //if (ActiveChatWindow)
            //ActiveChatWindow?.ToggleLoadMoreButton(!(ChatListener.Instance._activeChatChannel.FirstMsgIndex <= 1));

        ForceRebuild(chatWindow.RectTransform);

        return chatMessageInstance;
    }

    /// <summary>
    /// Changes the currently active chat window.
    /// </summary>
    /// <param name="channelType">Type of chat to set active</param>
    /// <remarks>
    /// This function gets called when Global, Clan or Country button gets pressed in the UI.
    /// </remarks>
    internal void OnChatWindowChanged(ChatChannelType channelType)
    {
        ChatWindow chatWindow = Array.Find(_chatWindows, item => item.ChannelType == channelType);
        int activeChatChannelIndex = Array.FindIndex(ChatListener.Instance.ChatChannels, item => item.ChatChannelType == channelType);

        // Changes the desired window active and disable others
        foreach (ChatWindow window in _chatWindows)
        {
            window.gameObject.SetActive(!(window != chatWindow));
            window.ChangeToWindowButton.interactable = (window != chatWindow);
        }

        ActiveChatWindow = chatWindow;
        //ChatListener.Instance._activeChatChannel = ChatListener.Instance._chatChannels[activeChatChannelIndex];
        //ActiveChatWindow.ToggleLoadMoreButton(!(ChatListener.Instance._activeChatChannel.FirstMsgIndex <= 1));
        _chatroomNameText.text = ChatListener.Instance.GetActiveChannel.ChannelName;
        ForceRebuild(ActiveChatWindow.RectTransform);

        // Refreshes the preview chat (the small chat) window messages
        //ChatListener.Instance?.ChatPreviewController?.OnActiveChatWindowChange(ChatListener.Instance._activeChatChannel);
    }

    /// <summary>
    /// Changes the currently active clan chat.
    /// </summary>
    /// <param name="chatName"></param>
    public void OnClanChatChanged(string chatName)
    {
        int clanChannelIndex = Array.FindIndex(ChatListener.Instance.ChatChannels, item => item.ChatChannelType == ChatChannelType.Clan);

        ChatChannel clanChannel = ChatListener.Instance.ChatChannels[clanChannelIndex];
        clanChannel.Reset();
        //clanChannel.ChannelName = chatName;

        _chatroomNameText.text = ChatListener.Instance.GetActiveChannel.ChannelName;
    }

    public void ShowErrorMessage(string errorText)
    {
        GameObject errorInstance = Instantiate(_errorPrefab, _errorPanel.transform);
        ChatError errorComponent = errorInstance.GetComponent<ChatError>();
        errorComponent.SetErrorText(errorText);
    }

    public void DeleteChatHistory(ChatChannel channel)
    {
        ChatWindow chatWindow = Array.Find(_chatWindows, item => item.ChannelType == channel.ChatChannelType);
        RectTransform rectTransform = chatWindow.RectTransform;

        foreach (Transform child in rectTransform)
        {
            if (child.tag == "ChatMessage")
                Destroy(child.gameObject);
        }

        chatWindow.LoadMoreMessagesButton.gameObject.SetActive(false);
        ForceRebuild(ActiveChatWindow.RectTransform);
    }

    private void ForceRebuild(RectTransform chatWindowRect) { LayoutRebuilder.ForceRebuildLayoutImmediate(chatWindowRect); }
}
