using System;
using System.Collections.Generic;
using Altzone.Scripts.Common;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.UI;

public class EmotionSelectorPopupScript : AltMonoBehaviour
{
    // The popup that has all the mood options.
    [SerializeField] private GameObject _popupPrefab;

    // The mood buttons that are in the popup.
    [SerializeField] private Button _loveButton;
    [SerializeField] private Button _playfulButton;
    [SerializeField] private Button _joyButton;
    [SerializeField] private Button _sadButton;
    [SerializeField] private Button _angryButton;

    static bool _bSwitch = true;

    public static bool EmotionInsertedToday => !_bSwitch;

    // Creates the variable that is used to get the list of the moods.
    private PlayerData _playerData;

    public delegate void EmotionInsertFinished();
    public static event EmotionInsertFinished OnEmotionInsertFinished;


    private void Awake()
    {
        _bSwitch = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        // Gets the needed playerdata.
        StartCoroutine(GetPlayerData(data => _playerData = data));

        if (!string.IsNullOrWhiteSpace(_playerData.emotionSelectorDate))
        {
            // Checks if the player has given input the same day.
            if (DateTime.Parse(_playerData.emotionSelectorDate) == DateTime.Today) _bSwitch = false;
        }

        if (_bSwitch)
        {
            // Opens the popup unless the player has given input the same day.
            _popupPrefab.SetActive(_bSwitch);
        }
        else
        {
            OnEmotionInsertFinished?.Invoke();
        }

        // Listeners that listen what button has been pressed and does the method given.
        // The buttons have their own mood so its easier to add the mood to the list.
        _loveButton.onClick.AddListener(() => SaveMoodData(Emotion.Love));
        _playfulButton.onClick.AddListener(() => SaveMoodData(Emotion.Playful));
        _joyButton.onClick.AddListener(() => SaveMoodData(Emotion.Joy));
        _sadButton.onClick.AddListener(() => SaveMoodData(Emotion.Sorrow));
        _angryButton.onClick.AddListener(() => SaveMoodData(Emotion.Anger));
    }

    // Closes the popup.
    public void ClosePopup()
    {
        _popupPrefab.SetActive(false);
    }

    // Saves the mood that the player has chosen.
    public void SaveMoodData(Emotion emotion)
    {
        List<Emotion> data = _playerData.playerDataEmotionList;
        int days = 7;
        if (!string.IsNullOrWhiteSpace(_playerData.emotionSelectorDate))
        {
            TimeSpan span = DateTime.Today - DateTime.Parse(_playerData.emotionSelectorDate);
            days = span.Days;
            if(days > 7) days = 7;
        }

        for (int i = days-1; i > 0; i--)
        {
            // Removes the last item in the list of moods
            data.RemoveAt(data.Count - 1);

            // Adds the newest item to the list of emotions.
            data.Insert(0, Emotion.Blank);
        }

        // Removes the last item in the list of moods
        data.RemoveAt(data.Count-1);

        // Adds the newest item to the list of emotions.
        data.Insert(0, emotion);

        _playerData.emotionSelectorDate = DateTime.Today.ToString();

        _playerData.daysBetweenInput = days.ToString();

        _playerData.playerDataEmotionList = data;

        Debug.Log(days);

        // Saves the playerdata that has been changed.
        StartCoroutine(SavePlayerData(_playerData, null));
        _bSwitch = false;
        ClosePopup();
        OnEmotionInsertFinished?.Invoke();
    }
}
