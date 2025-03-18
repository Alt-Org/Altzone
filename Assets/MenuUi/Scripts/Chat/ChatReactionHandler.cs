using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatReactionHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counter;
    [SerializeField] public Image _reactionImage;

    public int _messageID;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetReactionInfo(Sprite image, int counter, int messageID)
    {
        _reactionImage.sprite = image;
        _counter.text = counter.ToString();
        _messageID = messageID;
    }
}
