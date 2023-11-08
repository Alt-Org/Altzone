using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

// TODO: Optimize:
// - Do not always load all the messages, use some kind of index
// - Maybe show only a set amount of messages and use a 'Load More' button to load older messages

public class ChatController : MonoBehaviour
{
    private ChatWindow _activeChatWindow;

    private const string DisconnectedText = "Connection status: <color=red>Disconnected</color>";
    private const string ConnectedText = "Connection status: <color=green>Connected</color>";

    [Header("Appearance")]
    [SerializeField] private int _fontSize;

    [Header("Top Bar")]
    [SerializeField] private TMP_Text _connectionStatusText;
    [SerializeField] private TMP_Dropdown _chatChannelDropdown;

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

    public ChatWindow ActiveChatWindow { get => _activeChatWindow; set => _activeChatWindow = value; }

    private void Awake()
    {
        #region Chat Window initialization

        _chatWindows = new ChatWindow[] { _globalChatWindow, _clanChatWindow, _countryChatWindow };

        _globalChatWindow.Channel = ChatListener.Instance._globalChatChannel;
        _clanChatWindow.Channel = ChatListener.Instance._clanChatChannel;
        _countryChatWindow.Channel = ChatListener.Instance._countryChatChannel;

        _globalChatWindow.DropdownOptions = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Global") };
        _clanChatWindow.DropdownOptions = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Clan 1"), new TMP_Dropdown.OptionData("Clan 2"), new TMP_Dropdown.OptionData("Clan 3") };
        _countryChatWindow.DropdownOptions = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Country 1"), new TMP_Dropdown.OptionData("Country 2"), new TMP_Dropdown.OptionData("Country 3") };

        _globalChatWindow.ChangeToWindowButton.onClick.AddListener(() => OnChatWindowChange(_globalChatWindow.Channel));
        _clanChatWindow.ChangeToWindowButton.onClick.AddListener(() => OnChatWindowChange(_clanChatWindow.Channel));
        _countryChatWindow.ChangeToWindowButton.onClick.AddListener(() => OnChatWindowChange(_countryChatWindow.Channel));

        #endregion

        _sendMessageButton.onClick.AddListener(() => SendChatMessage(_chatMessageInputField.text));

        _chatChannelDropdown.onValueChanged.AddListener(delegate
        {
            OnChatWindowChange(ActiveChatWindow.Channel);
        });
    }

    private void Start()
    {
        ChatListener.Instance.ChatController = this;

        if (ChatListener.Instance._chatMessages != null)
        {
            foreach (var message in ChatListener.Instance._chatMessages)
                InstantiateChatMessagePrefab(message);
        }

        OnChatWindowChange(ChatListener.Instance._activeChatChannel);
    }

    private void Update()
    {
        // So we can send messages with "enter" key on PC
        if (SystemInfo.deviceType == DeviceType.Desktop && _chatMessageInputField.IsActive() && Input.GetKeyDown(KeyCode.Return))
            SendChatMessage(_chatMessageInputField.text);

        if (ChatListener.Instance.ChatClient != null && ChatListener.Instance.ChatClient.CanChat)
            _connectionStatusText.text = ConnectedText;
        else
            _connectionStatusText.text = DisconnectedText;
    }

    internal void SendChatMessage(string message)
    {
        if (message == "")
            return;

        object[] dataToSend = new object[] { ChatListener.Instance._username, _chatMessageInputField.text, _moodDropdown.value, _activeChatWindow.Channel._chatChannelType };
        ChatListener.Instance.ChatClient.PublishMessage(ActiveChatWindow.Channel._channelName, dataToSend, true);
        _chatMessageInputField.text = null;
    }

    internal void InstantiateChatMessagePrefab(ChatMessage message)
    {
        ChatWindow chatWindow;

        // Finds the correct chat window by comparing the subscribed chat channel names with the chat channel value received from Photon Chat
        if (!(chatWindow = Array.Find(_chatWindows, item => item.Channel._channelName == message._channelName)))
            return;

        ChatMessagePrefab chatMessageInstance;

        if (message._username == ChatListener.Instance._username)
            chatMessageInstance = Instantiate(_chatMessageLocalPrefab, chatWindow.RectTransform).GetComponent<ChatMessagePrefab>();
        else
            chatMessageInstance = Instantiate(_chatMessageOthersPrefab, chatWindow.RectTransform).GetComponent<ChatMessagePrefab>();

        chatMessageInstance.SetName(message._username);
        chatMessageInstance.SetMessage(message._message);
        chatMessageInstance.SetMood(message._mood);
        chatMessageInstance.SetProfilePicture(message._chatChannelType);
        chatMessageInstance.SetFontSize(_fontSize);

        ForceRebuild(chatWindow.RectTransform);
    }

    internal void OnChatWindowChange(ChatChannel chatChannel)
    {
        ChatWindow chatWindow = Array.Find(_chatWindows, item => item.Channel._chatChannelType == chatChannel._chatChannelType);

        if (!ActiveChatWindow || chatChannel._chatChannelType != ActiveChatWindow.Channel._chatChannelType)
        {
            foreach (ChatWindow window in _chatWindows)
            {
                window.gameObject.SetActive(!(window != chatWindow));
                window.ChangeToWindowButton.interactable = (window != chatWindow);
            }

            _chatChannelDropdown.interactable = true;
            _chatChannelDropdown.ClearOptions();
            _chatChannelDropdown.AddOptions(chatWindow.DropdownOptions);

            if (chatWindow.Channel._chatChannelType == ChatListener.ChatChannelType.Global)
                _chatChannelDropdown.interactable = false;
            else
                _chatChannelDropdown.SetValueWithoutNotify((_chatChannelDropdown.options.FindIndex(option => option.text == chatWindow.Channel._channelName)));

            ActiveChatWindow = chatWindow;
            ChatListener.Instance._activeChatChannel = ActiveChatWindow.Channel;
            ForceRebuild(ActiveChatWindow.RectTransform);
        }
        else
        {
            ChatListener.Instance.ChatClient.Unsubscribe(new string[] { ActiveChatWindow.Channel._channelName });
            DeleteChatHistory(ActiveChatWindow.RectTransform);

            ActiveChatWindow = chatWindow;
            ActiveChatWindow.Channel._channelName = _chatChannelDropdown.options[_chatChannelDropdown.value].text;

            ChatListener.Instance._activeChatChannel = ActiveChatWindow.Channel;
            ChatListener.Instance.ChatClient.Subscribe(ActiveChatWindow.Channel._channelName);

            int index = Array.FindIndex(ChatListener.Instance._chatChannels, item => item._chatChannelType == chatChannel._chatChannelType);
            ChatListener.Instance._chatChannels[index] = ActiveChatWindow.Channel;

            if (ChatTester.Instance.isActive)
                ChatListener.Instance._chatChannels[index]._lastMsgIndex = 0;
        }

        ChatListener.Instance?.ChatPreviewController?.OnActiveChatWindowChange(ChatListener.Instance._activeChatChannel);
    }

    private void DeleteChatHistory(RectTransform activeChatWindowRect)
    {
        foreach (Transform child in activeChatWindowRect.transform)
            Destroy(child.gameObject);
    }

    private void ForceRebuild(RectTransform chatWindowRect) { LayoutRebuilder.ForceRebuildLayoutImmediate(chatWindowRect); }
}
