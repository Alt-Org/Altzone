using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatPreviewController : MonoBehaviour
{
    [Header("Amount of chat messages shown in chat preview window")]
    [Tooltip("The amount of messages shown in the chat box")] public int chatMessageAmount;
    [Tooltip("How much of the chat message will be shown before it is cut off")] public float textCutOffDistance;

    [Header("GameObjects")]
    [SerializeField] private GameObject chatPreviewMessagePrefab;
    [SerializeField] private GameObject noMessagesTextGameobject;
    [SerializeField] private GameObject _chatMessagesContainer;
    [SerializeField] private Button _toggleChatButton;
    [SerializeField] private TextMeshProUGUI _activeChatChannelText;

    private GameObject[] _chatMessageGameobjects;
    private bool _isEnabled;
    private Image _backgroundImage;

    [Header("Animations")]
    public AnimationClip _chatButtonShrinkAnim;
    public AnimationClip _chatButtonExpandAnim;
    private Animation _chatButtonAnim;

    public TextMeshProUGUI ActiveChatChannelText { get => _activeChatChannelText; set => _activeChatChannelText = value; }

    private void Awake()
    {
        _isEnabled = _chatMessagesContainer.activeInHierarchy;
        _backgroundImage = GetComponent<Image>();
        _toggleChatButton.onClick.AddListener(() => ToggleChatMessages(!_isEnabled));

        _chatButtonAnim = _toggleChatButton.GetComponent<Animation>();
        _chatMessageGameobjects = new GameObject[chatMessageAmount];

        for (int i = 0; i < chatMessageAmount; i++)
        {
            GameObject chatMessage = Instantiate(chatPreviewMessagePrefab, _chatMessagesContainer.transform);
            _chatMessageGameobjects[i] = chatMessage;
            chatMessage.GetComponentInChildren<Image>().color = Color.clear;
            chatMessage.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }

    private void Start()
    {
        ChatListener.Instance.ChatPreviewController = this;
        ChatListener.Instance.ChatPreviewController.OnActiveChatWindowChange(ChatListener.Instance._activeChatChannel);
        MessageReceived(ChatListener.Instance._activeChatChannel);
    }

    private void OnEnable()
    {
        if (ChatListener.Instance)
        {
            ChatListener.Instance.ChatPreviewController = this;
            OnActiveChatWindowChange(ChatListener.Instance._activeChatChannel);
            ToggleChatMessages(ChatListener.Instance._chatPreviewIsEnabled);
        }
    }

    private void OnDisable()
    {
        ChatListener.Instance.ChatPreviewController = null;
    }

    internal void ToggleChatMessages(bool value)
    {
        _backgroundImage.enabled = value;
        _chatMessagesContainer.SetActive(value);
        _isEnabled = value;
        ChatListener.Instance._chatPreviewIsEnabled = value;

        if ((value && _chatButtonAnim.clip != _chatButtonExpandAnim) || (!value && _chatButtonAnim.clip != _chatButtonShrinkAnim))
        {
            if (value)
                _chatButtonAnim.clip = _chatButtonExpandAnim;
            else
                _chatButtonAnim.clip = _chatButtonShrinkAnim;

            _chatButtonAnim.Play();
        }
    }

    internal void OnActiveChatWindowChange(ChatChannel chatChannel)
    {
        ClearChatMessages();
        MessageReceived(chatChannel);

        ChatListener.Instance._activeChatChannel = chatChannel;
        ActiveChatChannelText.text = $"{chatChannel._channelName}";
    }

    internal void ClearChatMessages()
    {
        for (int i = 0; i < _chatMessageGameobjects.Length; i++)
        {
            _chatMessageGameobjects[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
            _chatMessageGameobjects[i].GetComponentInChildren<Image>().color = Color.clear;
        }

        noMessagesTextGameobject.SetActive(true);
    }

    internal void MessageReceived(ChatChannel channel)
    {
        if (ChatListener.Instance._chatMessages.Count == 0)
            return;

        ChatMessage[] recentMessages = new ChatMessage[chatMessageAmount];
        int index = 0;

        // Checks the x amount of most recent chat messages that match the currently active chat channel name

        for (int i = ChatListener.Instance._chatMessages.Count - 1; i >= 0; i--)
        {
            ChatMessage message = ChatListener.Instance._chatMessages[i];

            if (message._channelName != channel._channelName)
                continue;

            recentMessages[index++] = message;

            if (index == chatMessageAmount)
                break;
        }

        // Inserts the chat message and shortens it to fit the chatbox window

        for (int i = 0; i < index; i++)
        {
            if (noMessagesTextGameobject.activeInHierarchy)
                noMessagesTextGameobject.SetActive(false);

            ChatMessagePrefab chatMessagePrefab = _chatMessageGameobjects[i].GetComponent<ChatMessagePrefab>();
            chatMessagePrefab.SetMessage(recentMessages[i]._message);
            chatMessagePrefab.SetProfilePicture(channel._chatChannelType);

            TextMeshProUGUI textMeshProUGUI = _chatMessageGameobjects[i].GetComponentInChildren<TextMeshProUGUI>();
            textMeshProUGUI.ForceMeshUpdate();

            string shortenedMessage = ShortenChatMessage(textMeshProUGUI.textInfo);

            chatMessagePrefab.SetMessage(shortenedMessage);
        }
    }

    private string ShortenChatMessage(TMP_TextInfo textInfo)
    {
        string returnString = string.Empty;

        for (int j = 0; j < textInfo.characterCount; ++j)
        {
            returnString += textInfo.characterInfo[j].character;

            if (!textInfo.characterInfo[j].isVisible || textInfo.characterInfo[j].bottomRight.x < textCutOffDistance)
                continue;

            StringBuilder sb = new StringBuilder(returnString.TrimEnd(' '));
            sb[returnString.Length - 1] = '.';
            sb[returnString.Length - 2] = '.';
            sb[returnString.Length - 3] = '.';
            returnString = sb.ToString();
            break;
        }

        return returnString;
    }
}
