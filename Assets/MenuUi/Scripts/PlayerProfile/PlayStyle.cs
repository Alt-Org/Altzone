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

    [Header("Styles per Language")]
    public string[] finnishStyles; // Finnish 
    public string[] englishStyles; // English 

    private int currentIndex = 0; // Tracks the selected style

    public int CurrentIndex
    {
        get => currentIndex;
        set => currentIndex = value;
    }

    private string[] CurrentStyles
    {
        get
        {
            if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
                return englishStyles;
            return finnishStyles; // Default to Finnish
        }
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
        var styles = CurrentStyles; // Pick the list based on language

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
        int count = Enum.GetNames(typeof(PlayStyles)).Length;
        currentIndex = (currentIndex - 1 + count) % count;
        UpdateStyleText();
        SaveCurrentIndex();
    }

    private void SelectNextStyle()
    {
        int count = Enum.GetNames(typeof(PlayStyles)).Length;
        currentIndex = (currentIndex + 1) % count;
        UpdateStyleText();
        SaveCurrentIndex();
    }

    // Saves the current index as a playerpref.
    private void SaveCurrentIndex()
    {
        PlayerPrefs.SetInt("CurrentPlayStyleIndex", currentIndex);
        PlayerPrefs.Save();

        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
    }
}
