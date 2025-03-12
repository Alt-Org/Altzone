using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatReactionHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counter;
    [SerializeField] private Image _reactionImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetReactionInfo(Sprite image, int counter)
    {
        _reactionImage.sprite = image;
        _counter.text = counter.ToString();
    }
}
