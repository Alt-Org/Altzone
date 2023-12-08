using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWindow : MonoBehaviour
{
    private List<TMP_Dropdown.OptionData> _dropdownOptions;
    [SerializeField] private Button _changeToWindowButton;
    [SerializeField] private Button _loadMoreMessagesButton;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private ChatListener.ChatChannelType _channelType;

    public List<TMP_Dropdown.OptionData> DropdownOptions { get => _dropdownOptions; set => _dropdownOptions = value; }
    public Button ChangeToWindowButton { get => _changeToWindowButton; set => _changeToWindowButton = value; }
    public Button LoadMoreMessagesButton { get => _loadMoreMessagesButton; set => _loadMoreMessagesButton = value; }
    public RectTransform RectTransform { get => _rectTransform; set => _rectTransform = value; }
    public ChatListener.ChatChannelType ChannelType { get => _channelType; set => _channelType = value; }

    public void GetChatMessagesFromServer()
    {
        ChatListener.Instance.GetChatHistoryFromServer(ChatListener.Instance._activeChatChannel, ChatListener.ServerHistoryCause.LoadMoreButtonClicked, null);
    }

    public void ToggleLoadMoreButton(bool value)
    {
        LoadMoreMessagesButton.gameObject.SetActive(value);
    }
}
