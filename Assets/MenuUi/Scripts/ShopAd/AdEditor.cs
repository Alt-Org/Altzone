using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;

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

    [SerializeField] private Image border1;
    [SerializeField] private Image border2;
    [SerializeField] private Image border3;
    [SerializeField] private Image border4;
    [SerializeField] private Image border5;
    [SerializeField] private Image border6;
    [SerializeField] private Image border7;
    [SerializeField] private Image border8;
    [SerializeField] private Image border9;
    [SerializeField] private Image border10;
    [SerializeField] private Image border11;

    public Transform ChooseEffect;
    public Transform ChooseBorder;
    public Button buttonEffect;
    public Button buttonBorder;

    void Start()    
    {
        buttonEffect.onClick.AddListener(() => BringToFront(ChooseEffect));
        buttonBorder.onClick.AddListener(() => BringToFront(ChooseBorder));
    }

    void BringToFront(Transform folder)
    {
        folder.SetAsLastSibling();
    }


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

    public void ChangeBorder1()
    {
        _borderImage.sprite = border1.sprite;
    }
    public void ChangeBorder2()
    {
        _borderImage.sprite = border2.sprite;
    }
    public void ChangeBorder3()
    {
        _borderImage.sprite = border3.sprite;
    }
    public void ChangeBorder4()
    {
        _borderImage.sprite = border4.sprite;
    }
    public void ChangeBorder5()
    {
        _borderImage.sprite = border5.sprite;
    }
    public void ChangeBorder6()
    {
        _borderImage.sprite = border6.sprite;
    }
    public void ChangeBorder7()
    {
        _borderImage.sprite = border7.sprite;
    }
    public void ChangeBorder8()
    {
        _borderImage.sprite = border8.sprite;
    }
    public void ChangeBorder9()
    {
        _borderImage.sprite = border9.sprite;
    }
    public void ChangeBorder10()
    {
        _borderImage.sprite = border10.sprite;
    }
    public void ChangeBorder11()
    {
        _borderImage.sprite = border11.sprite;
    }

    public void CloseEditor()
    {
        gameObject.SetActive(false);
    }
}
