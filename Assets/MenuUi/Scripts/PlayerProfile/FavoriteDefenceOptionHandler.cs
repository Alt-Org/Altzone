using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FavoriteDefenceOptionHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;

    public Button SetData(string text, Sprite sprite)
    {
        _text.text = text;
        _image.sprite = sprite;
        return _button;
    }
}
