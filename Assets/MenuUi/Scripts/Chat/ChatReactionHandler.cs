using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatReactionHandler : MonoBehaviour
{
    [SerializeField] private GameObject _messageReaction;
    [SerializeField] private TextMeshProUGUI _counter;
    [SerializeField] public Image _reactionImage;

    [SerializeField] public Button _button;

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

        var reactionBackground = _messageReaction.GetComponentInChildren<Image>();
        var selectedColor = reactionBackground.color;
        selectedColor = Color.cyan;
        selectedColor.a = 0.3f;
        reactionBackground.color = selectedColor;   

        _selected = true;
    }

    public void Deselect()
    {
        _count--;
        _counter.text = _count.ToString();

        var reactionBackground = _messageReaction.GetComponentInChildren<Image>();
        var deselectedColor = reactionBackground.color;
        deselectedColor = Color.gray;
        deselectedColor.a = 0.5f;
        reactionBackground.color = deselectedColor;

        _selected = false;
    }
}
