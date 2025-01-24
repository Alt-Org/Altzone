using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdEditor: MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _effectImage;
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _borderImage;
   

    public List<Sprite> spriteChoices;
    private int counter;
    private int currentBorder = 0;


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
