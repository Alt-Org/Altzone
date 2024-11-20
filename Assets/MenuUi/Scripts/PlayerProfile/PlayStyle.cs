using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayStyle : MonoBehaviour
{
    public TextMeshProUGUI styleText; // TMP tekstikentt�
    public Button leftButton; // Vasen nappi
    public Button rightButton; // Oikea nappi
    public string[] styles; // Tyylivalikoima

    private int currentIndex = 0; // Seuraa valittua tyyli�

    void Start()
    {

        if (styleText != null)
        {
            styleText.raycastTarget = false;
        }


        leftButton.onClick.AddListener(SelectPreviousStyle);
        rightButton.onClick.AddListener(SelectNextStyle);

        UpdateStyleText();
    }

    // P�ivitt�� tyylitekstin
    private void UpdateStyleText()
    {
        if (styles.Length > 0)
        {
            styleText.text = styles[currentIndex];
        }
    }

    // Menee taaksep�in listassa
    private void SelectPreviousStyle()
    {
        currentIndex = (currentIndex - 1 + styles.Length) % styles.Length;
        UpdateStyleText();
    }

    // Menee eteenp�in listassa
    private void SelectNextStyle()
    {
        currentIndex = (currentIndex + 1) % styles.Length;
        UpdateStyleText();
    }
}
