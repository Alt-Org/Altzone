using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayStyle : MonoBehaviour
{
    public TextMeshProUGUI styleText; // TMP text field
    public Button leftButton; // Left button
    public Button rightButton; // Right button
    public string[] styles; // Style selection

    private int currentIndex = 0; // Tracks the selected style

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

        // Load saved index or default to 0
        currentIndex = PlayerPrefs.GetInt("CurrentPlayStyleIndex", 0);

        UpdateStyleText();
    }

    // Updates the style text
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

    private void SelectPreviousStyle()
    {
        currentIndex = (currentIndex - 1 + Enum.GetNames(typeof(PlayStyles)).Length) % Enum.GetNames(typeof(PlayStyles)).Length;
        UpdateStyleText();
        SaveCurrentIndex();
    }


    private void SelectNextStyle()
    {
        currentIndex = (currentIndex + 1) % Enum.GetNames(typeof(PlayStyles)).Length;
        UpdateStyleText();
        SaveCurrentIndex();
    }

    // Saves the currentindex as a playerpref.
    private void SaveCurrentIndex()
    {
        PlayerPrefs.SetInt("CurrentPlayStyleIndex", currentIndex);
        PlayerPrefs.Save();

        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
    }
}
