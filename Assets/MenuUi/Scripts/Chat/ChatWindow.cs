using Altzone.Scripts.Chat;
using UnityEngine;
using UnityEngine.UI;
public class ChatWindow : MonoBehaviour
{
    [SerializeField] private Button _changeToWindowButton;
    [SerializeField] private Button _loadMoreMessagesButton;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private ChatChannelType _channelType;

    public Button ChangeToWindowButton { get => _changeToWindowButton; set => _changeToWindowButton = value; }
    public Button LoadMoreMessagesButton { get => _loadMoreMessagesButton; set => _loadMoreMessagesButton = value; }
    public RectTransform RectTransform { get => _rectTransform; set => _rectTransform = value; }
    public ChatChannelType ChannelType { get => _channelType; set => _channelType = value; }

    public void GetChatMessagesFromServer()
    {
        //ChatListener.Instance.GetChatHistoryFromServer(ChatListener.Instance._activeChatChannel, ChatListener.ServerHistoryLoadingCause.LoadMoreButtonClicked, null);
    }

    public void ToggleLoadMoreButton(bool value)
    {
        LoadMoreMessagesButton.gameObject.SetActive(value);
    }
}
