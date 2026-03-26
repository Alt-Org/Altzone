using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using UnityEngine;
using UnityEngine.UI;

public class ReactionObjectHandler : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _image;
    private Mood _mood;
    private string _messageId;

    public Mood Mood => _mood;
    public string Id => _messageId;

    public delegate void ReactionPressed(string id, Mood mood);
    public static event ReactionPressed OnReactionPressed;

    public void SetInfo(Mood mood, Sprite sprite, string messageId)
    {
         _messageId = messageId;
        _mood = mood;
        _image.sprite = sprite;
        _button.onClick.AddListener(ReactionSelected);

    }

    private void ReactionSelected()
    {
        if (_mood != Mood.None)
        {
            OnReactionPressed?.Invoke(_messageId, _mood);
            ChatListener.Instance.SendReaction(_mood.ToString(), _messageId, ChatListener.Instance.ActiveChatChannel);
        }
    }

}
