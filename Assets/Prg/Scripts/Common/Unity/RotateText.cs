using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class RotateText : MonoBehaviour
{
    private RectTransform _textTransform;

    void Start()
    {
        _textTransform = GetComponent<RectTransform>();
        Rect rect = _textTransform.rect;
        _textTransform.anchorMin = new(0.5f,0.5f);
        _textTransform.anchorMax = new(0.5f, 0.5f);
        _textTransform.sizeDelta = new(rect.height, rect.width);
        _textTransform.rotation = Quaternion.Euler(0,0,90);
    }
}
