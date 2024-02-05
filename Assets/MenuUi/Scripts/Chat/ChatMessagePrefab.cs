using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatMessagePrefab : MonoBehaviour
{
    [SerializeField] private TMP_Text _senderNameText;
    [SerializeField] private TMP_Text _messageContentText;

    [SerializeField] private Image _backgroundImage = null;
    [SerializeField] private Image _profileImage = null;
    [SerializeField] private Image _moodImage = null;

    internal void SetName(string value)
    {
        _senderNameText.text = value;
    }

    internal void SetMessage(string message)
    {
        _messageContentText.text = message;
    }

    internal void SetMood(ChatListener.Mood mood)
    {
        Sprite sprite = Resources.Load<Sprite>("test-emojis/" + mood.ToString().ToLower());
        _moodImage.sprite = sprite;
    }

    internal void SetFontSize(int size) { _messageContentText.fontSize = size; }

    internal void SetProfilePicture(ChatListener.ChatChannelType chatChannelType)
    {
        string location = "";

        if (chatChannelType == ChatListener.ChatChannelType.Global)
        {
            location = "test-profilepicture/countrypicture";
        }
        else if (chatChannelType == ChatListener.ChatChannelType.Clan)
        {
            location = "test-profilepicture/profilepicture";
        }
        else if (chatChannelType == ChatListener.ChatChannelType.Country)
        {
            location = "test-profilepicture/clanpicture";
        }

        Sprite sprite = Resources.Load<Sprite>(location);

        if (sprite != null)
            _profileImage.sprite = sprite;

        _profileImage.color = Color.white;
    }

    internal void SetErrorColor()
    {
        _backgroundImage.color = Color.red;
    }
}
