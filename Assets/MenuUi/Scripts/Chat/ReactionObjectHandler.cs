using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using UnityEngine;
using UnityEngine.UI;
using static MessageReactionsHandler;

public class ReactionObjectHandler : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _image;
    [SerializeField] private Mood _mood;
    private bool _selected;
    private string _messageId;
    
    public Mood Mood => _mood;
    public string Id => _messageId;
    public bool Selected => _selected;

    public delegate void ReactionPressed(string id, Mood mood);
    public static event ReactionPressed OnReactionPressed;


    public void SetInfo(Mood mood, Sprite sprite, string messageId)
    {
         _messageId = messageId;
        _mood = mood;
        _image.sprite = sprite;
        _button.onClick.AddListener(ReactionSelected);

    }

    public void SetInfo(ReactionObject reaction, string messageId)
    {
        _messageId = messageId;
        _mood = reaction.Mood;
        _image.sprite = reaction.Sprite;
        _selected = reaction.Selected;
        reaction.OnSelectedStatusChanged += ToggleSelection;
        _button.onClick.AddListener(ReactionSelected);

    }

    private void ToggleSelection(Mood mood, bool selected)
    {
        _selected = selected;
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
