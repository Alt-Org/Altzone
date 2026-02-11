using System.Collections.Generic;
using Altzone.Scripts.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ServerChatMessage;

public class ChatReactionHandler : MonoBehaviour
{
    [SerializeField] private GameObject _messageReaction;
    [SerializeField] private TextMeshProUGUI _counter;
    [SerializeField] private Image _reactionImage;
    [SerializeField] private Button _button;
    [SerializeField] private LongClickButton _longClickButton;

    // Public getters
    public Image ReactionImage => _reactionImage;
    public Button Button => _button;
    public LongClickButton LongClickButton => _longClickButton;
    public Mood Mood => _mood;
    public string MessageID => _messageID;
    public int Count => _count;
    public bool Selected => _selected;

    private Mood _mood;
    private string _messageID;
    private int _count = 0;
    private bool _selected;
    private List<ReactionSenders> _reactioners;
    public static ChatReactionHandler Instance;

    public void SetReactionInfo(Sprite image, string messageID, Mood mood)
    {
        _reactionImage.sprite = image;
        _messageID = messageID;
        _mood = mood;
        _reactioners = new();
    }

    public void AddReaction(ServerReactions reaction)
    {
        if (_reactioners.Find(c => c.Id == reaction.sender_id) != null) return;

        _reactioners.Add(new(reaction.sender_id, reaction.playerName));
        _count++;
        _counter.text = _count.ToString();
    }

    public void Select()
    {
        Image reactionBackground = _messageReaction.GetComponentInChildren<Image>();
        Color selectedColor;
        selectedColor = Color.cyan;
        selectedColor.a = 0.3f;
        reactionBackground.color = selectedColor;

        _selected = true;
    }

    public void RemoveReaction(ServerReactions reaction)
    {
        ReactionSenders reactioner = _reactioners.Find(c => c.Id == reaction.sender_id);
        if (_reactioners.Find(c => c.Id == reaction.sender_id) == null) return;
        _reactioners.Remove(reactioner);
        _count--;
        _counter.text = _count.ToString();
    }

    public void Deselect()
    {
        Image reactionBackground = _messageReaction.GetComponentInChildren<Image>();
        Color deselectedColor;
        deselectedColor = Color.gray;
        deselectedColor.a = 0.5f;
        reactionBackground.color = deselectedColor;

        _selected = false;
    }

    public class ReactionSenders
    {
        private string _id;
        private string _playerName;

        public string Id { get => _id; }
        public string PlayerName { get => _playerName; }

        public ReactionSenders(string id, string playerName)
        {
            _id = id;
            _playerName = playerName;
        }
    }
}
