using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatReactionHandler : MonoBehaviour
{
    [SerializeField] private GameObject _messageReaction;
    [SerializeField] private TextMeshProUGUI _counter;
    public Image _reactionImage;
    public Button _button;
    public LongClickButton _longClickButton;

    public int _messageID;
    public int _count = 0;
    public bool _selected;

    public void SetReactionInfo(Sprite image, int messageID)
    {
        _reactionImage.sprite = image;
        _messageID = messageID;

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
