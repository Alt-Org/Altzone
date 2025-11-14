using Altzone.Scripts.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private Mood _mood;
    public string _messageID;
    public int _count = 0;
    public bool _selected;

    public void SetReactionInfo(Sprite image, string messageID, Mood mood)
    {
        _reactionImage.sprite = image;
        _messageID = messageID;
        _mood = mood;

        Select();
    }

    public void Select()
    {
        _count++;
        _counter.text = _count.ToString();

        Image reactionBackground = _messageReaction.GetComponentInChildren<Image>();
        Color selectedColor;
        selectedColor = Color.cyan;
        selectedColor.a = 0.3f;
        reactionBackground.color = selectedColor;   

        _selected = true;
    }

    public void Deselect()
    {
        _count--;
        _counter.text = _count.ToString();

        Image reactionBackground = _messageReaction.GetComponentInChildren<Image>();
        Color deselectedColor;
        deselectedColor = Color.gray;
        deselectedColor.a = 0.5f;
        reactionBackground.color = deselectedColor;

        _selected = false;
    }
}
