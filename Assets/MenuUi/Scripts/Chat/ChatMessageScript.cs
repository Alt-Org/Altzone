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

    public float _BackgroundPadding = 30f; // Korkeus, jonka verran tausta kasvaa jokaisen lisärivin myötä.

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

    public void MessageSetHeight()
    {
        // Dynaamisesti muuttaa taustan korkeutta ja paneelia tekstin rivim��r�n mukaan.
        if (textBackground != null && messageText != null)
        {
            float lineCount = messageText.textInfo.lineCount; // Haetaan nykyinen rivim��r� tekstist�.

            if (lineCount != _lastLineCount)
            {
                //Old version
                //float newHeight = _initialHeight + (lineCount - 1) * heightStep;


                /// This is more better as prefferedHeight will take the height it needs to fit in with the text and
                /// _BackgroundPadding is for padding so that textBackground wouldn't be too short
                float newHeight = Mathf.Max(110f, messageText.preferredHeight + _BackgroundPadding);


                Debug.LogWarning("FIND ME " + messageText.preferredHeight);


                //Changes the message text position when there's more text
                if(messageText.preferredHeight > 50)
                {
                    messageText.alignment = TextAlignmentOptions.TopLeft;
                }
                textBackground.sizeDelta = new Vector2(textBackground.sizeDelta.x, newHeight);

                textBackground.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(textBackground.parent.GetComponent<RectTransform>().sizeDelta.x, newHeight);
                _lastLineCount = lineCount;


                _messageHandler._baseMessageBankerSize = new Vector2(_messageHandler._baseMessageSize.sizeDelta.x, newHeight);

                _messageHandler.SizeCall();
                ///Old Line Incase needed
                //float originalSpacing = _messageVerticalLayoutGroup.spacing;
                //float newSpacing = originalSpacing * 0.2f * (lineCount - 1);
                //_messageVerticalLayoutGroup.spacing += newSpacing;

                ///Uskoisin että me voidaan vaan käyttää verticallayouttia vaan
            }
        }
    }
}
