using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Chat;
using MenuUi.Scripts.AvatarEditor;
using Assets.Altzone.Scripts.Model.Poco.Player;
using System.Text;
using System.Collections;

/// <summary>
/// ChatMessagePrefab contains references to children of an instantiated chat message prefab.
/// </summary>
public class ChatMessagePrefab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _senderNameText;
    [SerializeField] private TextMeshProUGUI _messageContentText;

    [SerializeField] private Image _backgroundImage = null;
    [SerializeField] private Image _profileImage = null;
    [SerializeField] private AvatarFaceLoader _faceAvatar = null;
    [SerializeField] private Image _moodImage = null;

    internal void SetInfo(ChatMessage message)
    {
        SetMessage(message.Message);
        SetProfilePicture(message.Avatar);
        SetMood(message.Mood);
    }

    internal void SetName(string value)
    {
        _senderNameText.text = value;
    }

    internal void SetMessage(string message)
    {
        _messageContentText.text = message;
        //StartCoroutine(SetShortenedMessageOnDelay(_messageContentText));
    }


    /// <summary>
    /// Sets the correct emoji and message background for the chat message instance.
    /// </summary>
    /// <<param name="mood">Mood value received from the server/Photon Chat</param>
    internal void SetMood(Mood mood)
    {
        switch (mood)
        {
            case Mood.Sad:
                _messageContentText.color = Color.blue;
                return;
            case Mood.Angry:
                _messageContentText.color = Color.red;
                return;
            case Mood.Happy:
                _messageContentText.color = Color.yellow;
                return;
            case Mood.Wink:
                _messageContentText.color = new(1, 0.4156f, 0);
                return;
            case Mood.Love:
                _messageContentText.color = new(0.929f,0.462f,0.6627f);
                return;
            default:
                return;
        }
    }

    internal void SetFontSize(int size) { _messageContentText.fontSize = size; }


    /// <summary>
    /// Sets the profile picture for the chat message instance.
    /// </summary>
    internal void SetProfilePicture(AvatarData avatar)
    {
        _faceAvatar.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(avatar));
    }

    internal void SetErrorColor()
    {
        _backgroundImage.color = Color.red;
    }

    private IEnumerator SetShortenedMessageOnDelay(TextMeshProUGUI textMeshProUGUI)
    {
        // Wait till the Unity UI has rendered so the message displays correctly
        yield return new WaitForEndOfFrame();

        textMeshProUGUI.ForceMeshUpdate();
        string shortenedMessage = ShortenChatMessage(textMeshProUGUI);
        textMeshProUGUI.text=shortenedMessage;

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
