using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdEditor : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _effectImage;
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _borderImage;

    [SerializeField] private Color orangeColor;
    [SerializeField] private Color yellowColor;
    [SerializeField] private Color lightGreenColor;
    [SerializeField] private Color lightBlueColor;
    [SerializeField] private Color blueColor;
    [SerializeField] private Color purpleColor;
    [SerializeField] private Color darkPinkColor;
    [SerializeField] private Color redColor;


    public void ChangeOrangeColor()
    {
        _backgroundImage.color = orangeColor;
    }
    public void ChangeYellowColor()
    {
        _backgroundImage.color = yellowColor;
    }
    public void ChangeLightGreenColor()
    {
        _backgroundImage.color = lightGreenColor;
    }

    public void ChangeLightBlueColor()
    {
        _backgroundImage.color = lightBlueColor;
    }
    public void ChangeBlueColor()
    {
        _backgroundImage.color = blueColor;
    }
    public void ChangePurpleColor()
    {
        _backgroundImage.color = purpleColor;
    }
    public void ChangeDarkPinkColor()
    {
        _backgroundImage.color = darkPinkColor;
    }
    public void ChangeRedColor()
    {
        _backgroundImage.color = redColor;
    }

    public void CloseEditor()
    {
        gameObject.SetActive(false);
    }
}
