using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
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

    public int CurrentIndex
    {
        get => currentIndex;
        set => currentIndex = value;
    }

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
        if (Enum.GetNames(typeof(PlayStyles)).Length > 0)
        {
            if (styles.Length > currentIndex)
            {
                styleText.text = styles[currentIndex];
            }
            else
            {
                styleText.text = ((PlayStyles)currentIndex).ToString();
            }
        }
    }

    // Menee taaksep�in listassa
    private void SelectPreviousStyle()
    {
        currentIndex = (currentIndex - 1 + Enum.GetNames(typeof(PlayStyles)).Length) % Enum.GetNames(typeof(PlayStyles)).Length;
        UpdateStyleText();
    }

    // Menee eteenp�in listassa
    private void SelectNextStyle()
    {
        currentIndex = (currentIndex + 1) % Enum.GetNames(typeof(PlayStyles)).Length;
        UpdateStyleText();
    }
}
