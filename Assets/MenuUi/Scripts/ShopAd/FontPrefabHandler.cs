using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class FontPrefabHandler : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _text;

    public void SetFont(AdFontObject font)
    {
        _image.preserveAspect = true;
        _text.font = font.Font;
    }
}
