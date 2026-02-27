using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleStoryLineHandler : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private RectTransform _textBackground;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Image _lineSprite;

    [SerializeField] private float heightStep = 40f; // Korkeus, jonka verran tausta kasvaa jokaisen lisärivin myötä.

    private int _lastLineCount = 0; // Tallentaa viimeksi lasketun rivimäärän.
    private float _initialHeight; // Alkuperäinen taustan korkeus, joka asetetaan alussa.


    private void Start()
    {
        // Alustetaan alkuperäinen taustan korkeus ja paneelin koko.
        if (_textBackground != null)
        {
            _initialHeight = _textBackground.sizeDelta.y;
        }
    }

    public void SetText(Sprite sprite, string text)
    {
        if(!gameObject.activeSelf) gameObject.SetActive(true);
        _lineSprite.sprite = sprite;
        _messageText.text = text;
        StartCoroutine(MessageSetHeight());
    }

    public void CloseTextBox()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator MessageSetHeight()
    {
        yield return new WaitForEndOfFrame();
        // Dynaamisesti muuttaa taustan korkeutta ja paneelia tekstin rivimäärän mukaan.
        if (_textBackground != null && _messageText != null)
        {
            int lineCount = _messageText.textInfo.lineCount; // Haetaan nykyinen rivimäärä tekstistä.

            if (lineCount != _lastLineCount)
            {
                float newHeight = _initialHeight + (lineCount - 1) * heightStep;

                _textBackground.sizeDelta = new Vector2(_textBackground.sizeDelta.x, newHeight);

                _lastLineCount = lineCount;
            }
        }
    }
}
