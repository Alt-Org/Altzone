using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using Altzone.Scripts.Common;
using UnityEngine;
using UnityEngine.UI;
using static MessageReactionsHandler;

public class ReactionObjectHandler : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _image;
    [SerializeField] private Emotion _mood;
    private bool _selected;
    private string _messageId;
    
    public Emotion Mood => _mood;
    public string Id => _messageId;
    public bool Selected => _selected;

    public delegate void ReactionPressed(string id, Emotion mood);
    public static event ReactionPressed OnReactionPressed;


    public void SetInfo(Emotion mood, Sprite sprite, string messageId)
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

    private void ToggleSelection(Emotion mood, bool selected)
    {
        _selected = selected;
    }

    private void ReactionSelected()
    {
        if (_mood != Emotion.Blank)
        {
            OnReactionPressed?.Invoke(_messageId, _mood);
            ChatListener.Instance.SendReaction(_mood.ToString(), _messageId, ChatListener.Instance.ActiveChatChannel);
        }
    }

}
