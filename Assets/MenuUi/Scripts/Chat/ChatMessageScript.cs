using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Deterministic;
using UnityEngine.UI;

public class ChatMessageScript : MonoBehaviour
{
    [Header("Objects")]
    public RectTransform textBackground;
    public TextMeshProUGUI messageText;
    public RectTransform panel;

    [SerializeField] private VerticalLayoutGroup _messageVerticalLayoutGroup;

    public float heightStep = 30f; // Korkeus, jonka verran tausta kasvaa jokaisen lis�rivin my�t�.

    private int _lastLineCount = 0; // Tallentaa viimeksi lasketun rivim��r�n.
    private float _initialHeight; // Alkuper�inen taustan korkeus, joka asetetaan alussa.


    private void Start()
    {
        // Alustetaan alkuper�inen taustan korkeus ja paneelin koko.
        if (textBackground != null && panel != null)
        {
            _initialHeight = textBackground.sizeDelta.y;

            panel.sizeDelta = new Vector2(panel.sizeDelta.x, _initialHeight);
        }
    }

    public void MessageSetHeight()
    {
        // Dynaamisesti muuttaa taustan korkeutta ja paneelia tekstin rivim��r�n mukaan.
        if (textBackground != null && messageText != null)
        {
            int lineCount = messageText.textInfo.lineCount; // Haetaan nykyinen rivim��r� tekstist�.

            if (lineCount != _lastLineCount)
            {
                float newHeight = _initialHeight + (lineCount - 1) * heightStep;

                textBackground.sizeDelta = new Vector2(textBackground.sizeDelta.x, newHeight);

                _lastLineCount = lineCount;

                // Adjust the spacing so that the reaction panel stays at the bottom line of the message
                float originalSpacing = _messageVerticalLayoutGroup.spacing;
                float newSpacing = originalSpacing * 0.2f * (lineCount - 1);
                _messageVerticalLayoutGroup.spacing += newSpacing;
            }
        }
    }
}
