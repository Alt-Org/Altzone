using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ChatPreviewController handles displaying chat messages in the small chat window in main menu.
/// /// </summary>
public class ChatPreviewController : MonoBehaviour
{
    [Header("Amount of chat messages shown in chat preview window")]
    [Tooltip("The amount of messages shown in the chat box")] public int chatMessageAmount;

    [Header("GameObjects")]
    [SerializeField] private GameObject chatPreviewMessagePrefab;
    [SerializeField] private GameObject noMessagesTextGameobject;
    [SerializeField] private GameObject _chatMessagesContainer;
    [SerializeField] private Button _toggleChatButton;
    //[SerializeField] private TextMeshProUGUI _activeChatChannelText;

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

    //public TextMeshProUGUI ActiveChatChannelText { get => _activeChatChannelText; set => _activeChatChannelText = value; }

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

        // These anchors are the positions of the image that shrinks and expands the chat when pressed. Default is default pos & Shrink is the shrunken pos.

        _chatButtonDefaultAnchors[0] = _chatButtonRect.anchorMin;
        _chatButtonDefaultAnchors[1] = _chatButtonRect.anchorMax;
        _chatButtonShrinkAnchors[0] = new Vector2(0.85f, 0.82f);
        _chatButtonShrinkAnchors[1] = new Vector2(1, 1.3f);
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

    private void Reset()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// Displays and hides the chat when Chat Preview image is pressed.
    /// </summary>
    /// <param name="value">Active or not</param>
    /// <param name="playAnimation">Determines if we play the animation or not</param>
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

        // Refresh the chat messages to see if we have received new messages while the chat was hidden.
        if (value)
            OnActiveChatWindowChange(ChatListener.Instance._activeChatChannel);
    }


    /// <summary>
    /// Deletes current chat messages and gets the lates ones.
    /// </summary>
    /// <param name="chatChannel">Chat channel from which the latest messages are fetched from.</param>
    internal void OnActiveChatWindowChange(ChatChannel chatChannel)
    {
        DeleteChatHistory();
        MessageReceived(chatChannel);

        //ActiveChatChannelText.text = $"{chatChannel._channelName}";
    }

    internal void DeleteChatHistory()
    {
        for (int i = 0; i < _chatMessageGameobjects.Length; i++)
        {
            _chatMessageGameobjects[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
            _chatMessageGameobjects[i].GetComponentInChildren<Image>().color = Color.clear;
        }

        noMessagesTextGameobject.SetActive(true); 
    }


    /// <summary>
    /// Retrieves the latest chat messages from _chatMessages list and displays them in the preview window.
    /// </summary>
    /// <param name="channel">Chat messages channel</param>
    internal void MessageReceived(ChatChannel channel)
    {
        if (ChatListener.Instance._chatMessages.Count == 0)
            return;

        ChatMessage[] recentMessages = new ChatMessage[chatMessageAmount];
        int index = 0;

        // Checks the x amount of most recent chat messages that match the given chat channel name

        for (int i = ChatListener.Instance._chatMessages.Count - 1; i >= 0; i--)
        {
            ChatMessage message = ChatListener.Instance._chatMessages[i];

            if (message._channel._channelName != channel._channelName)
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

    /// <summary>
    /// Inserts the chat messages to the Chat Preview window on delay.
    /// </summary>
    /// <param name="chatMessagePrefab">Chat message prefab</param>
    /// <param name="textMeshProUGUI">TextMeshProUGUI component of _chatMessageGameobject</param>
    /// <param name="message">Chat message string</param>
    /// <returns></returns>
    private IEnumerator SetShortenedMessageOnDelay(ChatMessagePrefab chatMessagePrefab, TextMeshProUGUI textMeshProUGUI,  string message)
    {
        // Wait till the Unity UI has rendered so the message displays correctly
        yield return new WaitForEndOfFrame();

        chatMessagePrefab.SetMessage(message);
        textMeshProUGUI.ForceMeshUpdate();
        string shortenedMessage = ShortenChatMessage(textMeshProUGUI);
        chatMessagePrefab.SetMessage(shortenedMessage);

    }


    /// <summary>
    /// Sets the last three characters to dots so they fit in chat preview message container.
    /// </summary>
    /// <param name="text">TextMeshProUGUI component of chat message</param>
    /// <returns>Shortened message</returns>
    private string ShortenChatMessage(TextMeshProUGUI text)
    {
        string returnString = string.Empty;
        Vector2 size = text.GetComponent<RectTransform>().rect.size;

        // If the message is shorter than the chat preview container, return the original text.
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
