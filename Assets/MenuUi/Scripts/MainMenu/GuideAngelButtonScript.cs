using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideAngelButtonScript : MonoBehaviour
{

    [SerializeField] private GameObject speechBubble;

    private bool isBubbleOpen;


    public void BubbleOpenerCloser()
    {
        isBubbleOpen = speechBubble.activeSelf;
        if (isBubbleOpen)
        {
            speechBubble.SetActive(false);
        }
        else if (!isBubbleOpen)
        {
            speechBubble.SetActive(true);
        }

    }
}
