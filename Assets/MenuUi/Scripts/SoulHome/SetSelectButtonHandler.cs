using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetSelectButtonHandler : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _setFontSprite;
    [SerializeField] private TextMeshProUGUI _setName;
    public Button Button => _button;

    public void SetFontSprite(Sprite sprite)
    {
        _setFontSprite.sprite = sprite;
        _setFontSprite.gameObject.SetActive(true);
        _setName.gameObject.SetActive(false);
    }

    public void SetSetName(string setName)
    {
        _setName.text = setName;
        _setName.gameObject.SetActive(true);
        _setFontSprite.gameObject.SetActive(false);
    }
}
