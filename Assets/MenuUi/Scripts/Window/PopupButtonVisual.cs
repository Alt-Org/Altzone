using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupButtonVisual : MonoBehaviour
{
    private Transform _glowSprite;
    private void Awake()
    {
        _glowSprite = transform.Find("Glow");
        if (_glowSprite != null)
             _glowSprite.gameObject.SetActive(false);
    }//awake

    public void ButtonSelected(bool selected)
    {
        transform.localScale = selected ? Vector3.one * 1.2f : Vector3.one;

        if (_glowSprite != null)
            _glowSprite.gameObject.SetActive(selected);
    }//selected
}//class
