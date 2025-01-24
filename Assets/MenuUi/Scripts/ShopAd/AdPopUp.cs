using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdPopUp : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _borderImage;
    [SerializeField] private Image _effectImage;
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _titleText;


    public AdPopUp(Sprite background, Sprite border, Sprite effect, Sprite item, string title)
    {
        _backgroundImage.sprite = background;
        _borderImage.sprite = border;
        _effectImage.sprite = effect;
        _itemImage.sprite = item;
        _titleText.text = title;
    }


    public void CloseAd()
    {
        Destroy(gameObject);
    }
}
