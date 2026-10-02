using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Deterministic;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatMessageScript : MonoBehaviour
{
    [Header("Objects")]
    public RectTransform textBackground;
    public TextMeshProUGUI messageText;
    public RectTransform panel;
    [SerializeField] private VerticalLayoutGroup _messageVerticalLayoutGroup;

    [SerializeField] private float _textMargins = 40f; // Korkeus, jonka verran tausta kasvaa jokaisen lisärivin myötä.

    private float _lastLineCount = 0; // Tallentaa viimeksi lasketun rivim��r�n.
    private float _initialHeight; // Alkuper�inen taustan korkeus, joka asetetaan alussa.
    public MessageObjectHandler _messageHandler;


    private void Start()
    {
        // Alustetaan alkuper�inen taustan korkeus ja paneelin koko.
        if (textBackground != null && panel != null)
        {
            _initialHeight = textBackground.sizeDelta.y;

            panel.sizeDelta = new Vector2(panel.sizeDelta.x, _initialHeight);
        }
    }

    public float MessageSetHeight()
    {
        // Dynaamisesti muuttaa taustan korkeutta ja paneelia tekstin rivim��r�n mukaan.
        if (textBackground != null && messageText != null)
        {
            float lineCount = messageText.textInfo.lineCount; // Haetaan nykyinen rivim��r� tekstist�.
            float currentHeight = textBackground.sizeDelta.y;

            if (lineCount != _lastLineCount)
            {
                /// This is more better as prefferedHeight will take the height it needs to fit in with the text and
                /// _textMargins is for the intended size of the margins so that text wouldn't end up too close to the edges.
                float newHeight = Mathf.Max(110f, messageText.preferredHeight + _textMargins);


                //Changes the message text position when there's more text
                if(messageText.preferredHeight > 50)
                {
                    messageText.alignment = TextAlignmentOptions.TopLeft;
                }
                textBackground.sizeDelta = new Vector2(textBackground.sizeDelta.x, newHeight);

                textBackground.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(textBackground.parent.GetComponent<RectTransform>().sizeDelta.x, newHeight);
                _lastLineCount = lineCount;

                return newHeight;
                ///Old Line Incase needed
                //float originalSpacing = _messageVerticalLayoutGroup.spacing;
                //float newSpacing = originalSpacing * 0.2f * (lineCount - 1);
                //_messageVerticalLayoutGroup.spacing += newSpacing;

                ///Uskoisin että me voidaan vaan käyttää verticallayouttia vaan
            }
            return currentHeight;
        }
        return 0;
    }
}
