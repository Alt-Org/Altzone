using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Chat;

/// <summary>
/// ChatMessagePrefab contains references to children of an instantiated chat message prefab.
/// </summary>
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


    /// <summary>
    /// Sets the correct emoji and message background for the chat message instance.
    /// </summary>
    /// <<param name="mood">Mood value received from the server/Photon Chat</param>
    internal void SetMood(Mood mood)
    {
        Sprite moodSprite = Resources.Load<Sprite>("test-emojis/" + mood.ToString().ToLower());
        _moodImage.sprite = moodSprite;

        //Sprite backgroundSprite = Resources.Load<Sprite>(null);
        //_backgroundImage.sprite = backgroundSprite;
    }

    internal void SetFontSize(int size) { _messageContentText.fontSize = size; }


    /// <summary>
    /// Sets the profile picture for the chat message instance.
    /// </summary>
    internal void SetProfilePicture(ChatChannelType chatChannelType)
    {
        string location = "";

        if (chatChannelType == ChatChannelType.Global)
        {
            location = "test-profilepicture/countrypicture";
        }
        else if (chatChannelType == ChatChannelType.Clan)
        {
            location = "test-profilepicture/profilepicture";
        }
        else if (chatChannelType == ChatChannelType.Country)
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
