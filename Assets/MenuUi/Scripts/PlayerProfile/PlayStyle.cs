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
    public List<StyleText> styles;  

    private int currentIndex = 0; // Tracks the selected style

    public PlayStyles CurrentStyle
    {
        get => styles[currentIndex].style;
        set => currentIndex = styles.FindIndex(style => style.style == value);
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
        if (styles.Count <= 0 || styles.Count <= currentIndex) return;

        styleText.text = styles[currentIndex].Text;

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

        DailyTaskProgressListener progressListener = GetComponent<DailyTaskProgressListener>();
        if (progressListener != null)
        {
            progressListener.UpdateProgress("1");
        }
    }

    public void RefreshUI()
    {
        UpdateStyleText();
    }

    [Serializable]
    public class StyleText
    {
        public PlayStyles style;
        public string finnishText;
        public string englishText;

        public string Text
        {
            get
            {
                if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
                    return englishText;
                return finnishText;
            }
        }
    }
}
