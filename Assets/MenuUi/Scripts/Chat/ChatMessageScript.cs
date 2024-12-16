using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Deterministic;

public class ChatMessageScript : MonoBehaviour
{
    [Header("Objects")]
    public RectTransform textBackground;
    public TextMeshProUGUI messageText;
    public RectTransform panel; 

    public float heightStep = 30f;

    private int lastLineCount = 0;
    private float initialHeight;

    private void Start()
    {
        if(textBackground != null && panel != null)
        {
            initialHeight = textBackground.sizeDelta.y;

            panel.sizeDelta = new Vector2(panel.sizeDelta.x, initialHeight);
        }
    }

    void Update()
    {
        MessageSetHeight();
    }

    public void MessageSetHeight()
    {
        if (textBackground != null && messageText != null)
        {
            int lineCount = messageText.textInfo.lineCount;

            if(lineCount != lastLineCount)
            {
                float newHeight = initialHeight + (lineCount - 1) * heightStep;

                textBackground.sizeDelta = new Vector2(textBackground.sizeDelta.x, newHeight);

                lastLineCount = lineCount;
            }
            
        }
    }

}
