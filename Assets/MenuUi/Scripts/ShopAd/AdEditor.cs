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

    [SerializeField] private Color blueColor;
    [SerializeField] private Color purpleColor;
    [SerializeField] private Color greenColor;
    [SerializeField] private Color orangeColor;
    [SerializeField] private Color whiteColor;

    public List<Sprite> borderChoices;
    public List<Sprite> itemChoices;
    private int borderCounter;
    private int itemCounter;
    private int currentBorder = 0;
    private int currentItem = 0;

    public void ChangeBlueColor()
    {
        _backgroundImage.color = blueColor;
    }
    public void ChangePurpleColor()
    {
        _backgroundImage.color = purpleColor;
    }
    public void ChangeGreenColor()
    {
        _backgroundImage.color = greenColor;
    }

    public void ChangetOrangeColor()
    {
        _backgroundImage.color = orangeColor;
    }
    public void ChangetWhiteColor()
    {
        _backgroundImage.color = whiteColor;
    }
    public void NextItem()
    {
        itemCounter++;
        if (itemCounter == 1)
        {

            currentItem++;
            itemCounter = 0;
            if (currentItem >= itemChoices.Count)
            {
                currentBorder = 0;
            }
            _itemImage.sprite = itemChoices[currentItem];
        }
    }
    public void NextBorder()
    {
        borderCounter++;
        if (borderCounter == 1)
        {

            currentBorder++;
            borderCounter = 0;
            if (currentBorder >= borderChoices.Count)
            {
                currentBorder = 0;
            }
            _borderImage.sprite = borderChoices[currentBorder];
        }
    }


    public void CloseEditor()
    {
        gameObject.SetActive(false);
    }
}
