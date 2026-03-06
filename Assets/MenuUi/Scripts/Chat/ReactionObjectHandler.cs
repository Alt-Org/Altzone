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
    [SerializeField] private MessageReactionsHandler _messageList;

    public Mood Mood => _mood;
    public string Id => _messageId;

    public delegate void ReactionPressed(string id, Mood mood);
    public static event ReactionPressed OnReactionPressed;

    private void Awake()
    {
        //Fetches the script what we need not certain if this is the best way to do it 100%
        _messageList = GetComponentInParent<MessageReactionsHandler>();
    }

    public void SetInfo(Mood mood, Sprite sprite, string messageId)
    {
         _messageId = messageId;
        _mood = mood;
        _image.sprite = sprite;
        _button.onClick.AddListener(ReactionSelected);

    }

    //i know right now this only works when the reaction is being added and does not revert back when the set reaction has been removed
    //As it litearlly can only be used when the object itself is enabled
    //Prob have to move this somewhere better place next time
    private void OnEnable()
    {
        //First checks if it is the same mood or not then checks if the set reaction has been selecter or not
        foreach (var c in _messageList._reactionList)
        {
            if (c.Mood == _mood)
            {
               gameObject.SetActive(!c.Selected);
            }
        }
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
