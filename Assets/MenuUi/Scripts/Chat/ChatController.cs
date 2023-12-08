using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

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

    [Header("ErrorPanel")]
    [SerializeField] internal GameObject _errorPanel;

    public ChatWindow ActiveChatWindow { get => _activeChatWindow; set => _activeChatWindow = value; }

    private void Awake()
    {
        #region Chat Window initialization

        _chatWindows = new ChatWindow[] { _globalChatWindow, _clanChatWindow, _countryChatWindow };
        _globalChatWindow.ChannelType = ChatListener.ChatChannelType.Global;
        _clanChatWindow.ChannelType = ChatListener.ChatChannelType.Clan;
        _countryChatWindow.ChannelType = ChatListener.ChatChannelType.Country;

        _globalChatWindow.DropdownOptions = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Global") };
        _clanChatWindow.DropdownOptions = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Clan 1"), new TMP_Dropdown.OptionData("Clan 2"), new TMP_Dropdown.OptionData("Clan 3") };
        _countryChatWindow.DropdownOptions = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Country 1"), new TMP_Dropdown.OptionData("Country 2"), new TMP_Dropdown.OptionData("Country 3") };

        _globalChatWindow.ChangeToWindowButton.onClick.AddListener(() => OnChatWindowChange(ChatListener.ChatChannelType.Global));
        _clanChatWindow.ChangeToWindowButton.onClick.AddListener(() => OnChatWindowChange(ChatListener.ChatChannelType.Clan));
        _countryChatWindow.ChangeToWindowButton.onClick.AddListener(() => OnChatWindowChange(ChatListener.ChatChannelType.Country));

        #endregion

        _sendMessageButton.onClick.AddListener(() => SendChatMessage(_chatMessageInputField.text));

        _chatChannelDropdown.onValueChanged.AddListener(delegate
        {
            OnChatWindowChange(ChatListener.Instance._activeChatChannel._chatChannelType);
        });
    }

    private void Start()
    {
        ChatListener.Instance.ChatController = this;

        OnChatWindowChange(ChatListener.Instance._activeChatChannel._chatChannelType);

        if (ChatListener.Instance._chatMessages != null)
        {
            foreach (var message in ChatListener.Instance._chatMessages)
                InstantiateChatMessagePrefab(message, false);
        }
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

        object[] dataToSend = new object[] {ChatListener.Instance._username, _chatMessageInputField.text, _moodDropdown.value, ChatListener.Instance._activeChatChannel._chatChannelType };
        ChatListener.Instance.ChatClient.PublishMessage(ChatListener.Instance._activeChatChannel._channelName, dataToSend, true);
        _chatMessageInputField.text = null;
    }

    internal void InstantiateChatMessagePrefab(ChatMessage message, bool instantiateOnTop)
    {
        ChatWindow chatWindow;

        if (!(chatWindow = Array.Find(_chatWindows, item => item.ChannelType == message._channel._chatChannelType)))
            return;

        ChatMessagePrefab chatMessageInstance;

        if (message._username == ChatListener.Instance._username)
            chatMessageInstance = Instantiate(_chatMessageLocalPrefab, chatWindow.RectTransform).GetComponent<ChatMessagePrefab>();
        else
            chatMessageInstance = Instantiate(_chatMessageOthersPrefab, chatWindow.RectTransform).GetComponent<ChatMessagePrefab>();

        chatMessageInstance.SetName(message._username);
        chatMessageInstance.SetMessage(message._message);
        //chatMessageInstance.SetMessage(message._id + " - " + message._message);
        chatMessageInstance.SetMood(message._mood);
        chatMessageInstance.SetProfilePicture(message._channel._chatChannelType);
        chatMessageInstance.SetFontSize(_fontSize);

        if (instantiateOnTop)
            chatMessageInstance.gameObject.transform.SetSiblingIndex(1);

        if (ActiveChatWindow)
            ActiveChatWindow?.ToggleLoadMoreButton(!(ChatListener.Instance._activeChatChannel._firstMsgIndex <= 1));

        ForceRebuild(chatWindow.RectTransform);
    }

    internal void OnChatWindowChange(ChatListener.ChatChannelType channelType)
    {
        ChatWindow chatWindow = Array.Find(_chatWindows, item => item.ChannelType == channelType);
        int activeChatChannelIndex = Array.FindIndex(ChatListener.Instance._chatChannels, item => item._chatChannelType == channelType);

        if (!ActiveChatWindow || channelType != ActiveChatWindow.ChannelType)
        {
            foreach (ChatWindow window in _chatWindows)
            {
                window.gameObject.SetActive(!(window != chatWindow));
                window.ChangeToWindowButton.interactable = (window != chatWindow);
            }

            _chatChannelDropdown.interactable = true;
            _chatChannelDropdown.ClearOptions();
            _chatChannelDropdown.AddOptions(chatWindow.DropdownOptions);

            if (chatWindow.ChannelType == ChatListener.ChatChannelType.Global)
                _chatChannelDropdown.interactable = false;
            else
                _chatChannelDropdown.SetValueWithoutNotify((_chatChannelDropdown.options.FindIndex(option => option.text == ChatListener.Instance._chatChannels[activeChatChannelIndex]._channelName)));

            ActiveChatWindow = chatWindow;
            ChatListener.Instance._activeChatChannel = ChatListener.Instance._chatChannels[activeChatChannelIndex];
            ActiveChatWindow.ToggleLoadMoreButton(!(ChatListener.Instance._activeChatChannel._firstMsgIndex <= 1));
            ForceRebuild(ActiveChatWindow.RectTransform);
        }
        else
        {
            ChatListener.Instance.ChatClient.Unsubscribe(new string[] { ChatListener.Instance._activeChatChannel._channelName });
            ChatListener.Instance.DeleteChatHistory(ChatListener.Instance._chatChannels[activeChatChannelIndex]);

            ActiveChatWindow = chatWindow;
            ChatChannel activeChannel = ChatListener.Instance._chatChannels[activeChatChannelIndex];
            activeChannel.Reset();
            activeChannel._channelName = _chatChannelDropdown.options[_chatChannelDropdown.value].text;

            ChatListener.Instance._activeChatChannel = activeChannel;
            ActiveChatWindow.ToggleLoadMoreButton(!(ChatListener.Instance._activeChatChannel._firstMsgIndex <= 1));
            StartCoroutine(ChatListener.Instance.GetChatHistoryAndSubscribe(new ChatChannel[] { activeChannel }, null));
        }

        ChatListener.Instance?.ChatPreviewController?.OnActiveChatWindowChange(ChatListener.Instance._activeChatChannel);
    }

    public void ShowErrorMessage(bool value, string errorText)
    {
        // Not implemented
    }

    public void DeleteChatHistory(ChatChannel channel)
    {
        ChatWindow chatWindow = Array.Find(_chatWindows, item => item.ChannelType == channel._chatChannelType);
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
