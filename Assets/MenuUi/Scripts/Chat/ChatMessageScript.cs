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

    public float heightStep = 30f; // Korkeus, jonka verran tausta kasvaa jokaisen lis�rivin my�t�.

    private int lastLineCount = 0; // Tallentaa viimeksi lasketun rivim��r�n.
    private float initialHeight; // Alkuper�inen taustan korkeus, joka asetetaan alussa.


    private void Start()
    {
        // Alustetaan alkuper�inen taustan korkeus ja paneelin koko.
        if (textBackground != null && panel != null)
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
        // Dynaamisesti muuttaa taustan korkeutta ja paneelia tekstin rivim��r�n mukaan.
        if (textBackground != null && messageText != null)
        {
            int lineCount = messageText.textInfo.lineCount; // Haetaan nykyinen rivim��r� tekstist�.

            if (lineCount != lastLineCount)
            {
                float newHeight = initialHeight + (lineCount - 1) * heightStep;

                textBackground.sizeDelta = new Vector2(textBackground.sizeDelta.x, newHeight);

                lastLineCount = lineCount;
            }
            
        }
    }

}
