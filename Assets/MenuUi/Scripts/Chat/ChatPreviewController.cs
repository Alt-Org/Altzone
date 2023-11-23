using System.Collections;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatPreviewController : MonoBehaviour
{
    [Header("Amount of chat messages shown in chat preview window")]
    [Tooltip("The amount of messages shown in the chat box")] public int chatMessageAmount;

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

    private RectTransform _chatButtonRect;
    private Vector2[] _chatButtonDefaultAnchors;
    private Vector2[] _chatButtonShrinkAnchors;

    public TextMeshProUGUI ActiveChatChannelText { get => _activeChatChannelText; set => _activeChatChannelText = value; }

    private void Awake()
    {
        _isEnabled = _chatMessagesContainer.activeInHierarchy;
        _backgroundImage = GetComponent<Image>();
        _toggleChatButton.onClick.AddListener(() => ToggleChatMessages(!_isEnabled, true));

        _chatButtonAnim = _toggleChatButton.GetComponent<Animation>();
        _chatMessageGameobjects = new GameObject[chatMessageAmount];

        for (int i = 0; i < chatMessageAmount; i++)
        {
            GameObject chatMessage = Instantiate(chatPreviewMessagePrefab, _chatMessagesContainer.transform);
            _chatMessageGameobjects[i] = chatMessage;
            chatMessage.GetComponentInChildren<Image>().color = Color.clear;
            chatMessage.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }

        _chatButtonRect = _toggleChatButton.gameObject.GetComponent<RectTransform>();

        _chatButtonDefaultAnchors = new Vector2[2];
        _chatButtonShrinkAnchors = new Vector2[2];

        _chatButtonDefaultAnchors[0] = _chatButtonRect.anchorMin;
        _chatButtonDefaultAnchors[1] = _chatButtonRect.anchorMax;
        _chatButtonShrinkAnchors[0] = new Vector2(0, 0.6f);
        _chatButtonShrinkAnchors[1] = new Vector2(0.15f, 1);
    }

    private void OnEnable()
    {
        if (ChatListener.Instance)
        {
            ChatListener.Instance.ChatPreviewController = this;
            OnActiveChatWindowChange(ChatListener.Instance._activeChatChannel);
            ToggleChatMessages(ChatListener.Instance._chatPreviewIsEnabled, false);
        }
    }

    private void OnDisable()
    {
        ChatListener.Instance.ChatPreviewController = null;
    }

    internal void ToggleChatMessages(bool value, bool playAnimation)
    {
        _backgroundImage.enabled = value;
        _chatMessagesContainer.SetActive(value);
        _isEnabled = value;
        ChatListener.Instance._chatPreviewIsEnabled = value;

        if (value)
            _chatButtonAnim.clip = _chatButtonExpandAnim;
        else
            _chatButtonAnim.clip = _chatButtonShrinkAnim;

        if (playAnimation)
        {
            _chatButtonAnim.Play();
        }
        else
        {
            if (value)
            {
                _chatButtonRect.anchorMin = _chatButtonDefaultAnchors[0];
                _chatButtonRect.anchorMax = _chatButtonDefaultAnchors[1];
            }
            else
            {
                _chatButtonRect.anchorMin = _chatButtonShrinkAnchors[0];
                _chatButtonRect.anchorMax = _chatButtonShrinkAnchors[1];
            }
        }

        if (value)
            OnActiveChatWindowChange(ChatListener.Instance._activeChatChannel);
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
            if (noMessagesTextGameobject.activeSelf)
                noMessagesTextGameobject.SetActive(false);

            TextMeshProUGUI textMeshProUGUI = _chatMessageGameobjects[i].GetComponentInChildren<TextMeshProUGUI>();

            ChatMessagePrefab chatMessagePrefab = _chatMessageGameobjects[i].GetComponent<ChatMessagePrefab>();
            chatMessagePrefab.SetProfilePicture(channel._chatChannelType);
            StartCoroutine(SetShortenedMessageOnDelay(chatMessagePrefab, textMeshProUGUI,  recentMessages[i]._message));
        }
    }

    private IEnumerator SetShortenedMessageOnDelay(ChatMessagePrefab chatMessagePrefab, TextMeshProUGUI textMeshProUGUI,  string message)
    {
        yield return new WaitForEndOfFrame();

        chatMessagePrefab.SetMessage(message);
        textMeshProUGUI.ForceMeshUpdate();
        string shortenedMessage = ShortenChatMessage(textMeshProUGUI);
        chatMessagePrefab.SetMessage(shortenedMessage);

    }

    private string ShortenChatMessage(TextMeshProUGUI text)
    {
        string returnString = string.Empty;
        Vector2 size = text.GetComponent<RectTransform>().rect.size;

        if (text.preferredWidth < size.x)
            return text.text;

        for (int i = 0; i < text.textInfo.characterCount; i++)
        {
            if (text.textInfo.characterInfo[i].bottomLeft.x > size.x)
                break;

            returnString += text.textInfo.characterInfo[i].character;
        }

        returnString = returnString.Replace("\n", "").Replace("\r", "");

        StringBuilder sb = new StringBuilder(returnString);
        sb[returnString.Length - 1] = '.';
        sb[returnString.Length - 2] = '.';
        sb[returnString.Length - 3] = '.';
        returnString = sb.ToString();

        return returnString;
    }
}
