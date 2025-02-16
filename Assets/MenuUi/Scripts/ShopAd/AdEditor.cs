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

    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;
    [SerializeField] private Color greenColor;
    [SerializeField] private Color yellowColor;

    public List<Sprite> spriteChoices;
    private int counter;
    private int currentBorder = 0;

    public void ChangeRedColor()
    {
        _backgroundImage.color = redColor;
    }
    public void ChangeBlueColor()
    {
        _backgroundImage.color = blueColor;
    }
    public void ChangeGreenColor()
    {
        _backgroundImage.color = greenColor;
    }

    public void ChangetYellowColor()
    {
        _backgroundImage.color = yellowColor;
    }
    public void NextBorder()
    {
        counter++;
        if (counter == 1)
        {

            currentBorder++;
            counter = 0;
            if (currentBorder >= spriteChoices.Count)
            {
                currentBorder = 0;
            }
            _borderImage.sprite = spriteChoices[currentBorder];
        }
    }
    

    public void CloseEditor()
    {
        gameObject.SetActive(false);
    }
}
