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
    [SerializeField] private RectTransform _textBackground;
    [SerializeField] private TextMeshProUGUI _messageText;

    [SerializeField] private float _textMargins = 40f; // Korkeus, jonka verran tausta kasvaa jokaisen lisärivin myötä.

    private float _lastLineCount = 0; // Tallentaa viimeksi lasketun rivim��r�n.

    public float MessageSetHeight()
    {
        // Dynaamisesti muuttaa taustan korkeutta ja paneelia tekstin rivim��r�n mukaan.
        if (_textBackground != null && _messageText != null)
        {
            float lineCount = _messageText.textInfo.lineCount; // Haetaan nykyinen rivim��r� tekstist�.
            float currentHeight = _textBackground.sizeDelta.y;

            if (lineCount != _lastLineCount)
            {
                /// This is more better as prefferedHeight will take the height it needs to fit in with the text and
                /// _textMargins is for the intended size of the margins so that text wouldn't end up too close to the edges.
                float newHeight = Mathf.Max(110f, _messageText.preferredHeight + _textMargins);

                //Changes the message text position when there's more text
                if(_messageText.preferredHeight > 50)
                {
                    _messageText.alignment = TextAlignmentOptions.TopLeft;
                }
                _textBackground.sizeDelta = new Vector2(_textBackground.sizeDelta.x, newHeight);

                _textBackground.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(_textBackground.parent.GetComponent<RectTransform>().sizeDelta.x, newHeight);
                _lastLineCount = lineCount;

                return newHeight;
            }
            return currentHeight;
        }
        return 0;
    }
}
