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

    // Creates the variable that is used to get the list of the moods.
    private PlayerData _playerData;

    // Start is called before the first frame update
    void Start()
    {
        // Opens the popup.
        _popupPrefab.SetActive(true);

        // Gets the needed playerdata
        StartCoroutine(GetPlayerData(data => _playerData = data));

        // Listeners that listen what button has been pressed and does the method given.
        // The buttons have their own mood so its easier to add the mood to the list.
        _loveButton.onClick.AddListener(() => SaveMoodData(Emotion.Love));
        _playfulButton.onClick.AddListener(() => SaveMoodData(Emotion.Playful));
        _joyButton.onClick.AddListener(() => SaveMoodData(Emotion.Joy));
        _sadButton.onClick.AddListener(() => SaveMoodData(Emotion.Sad));
        _angryButton.onClick.AddListener(() => SaveMoodData(Emotion.Angry));
    }

    // Closes the popup.
    public void ClosePopup()
    {
        _popupPrefab.SetActive(false);
    }

    // Saves the mood that the player has chosen.
    public void SaveMoodData(Emotion emotion)
    {
        List<Enum> data = _playerData.playerDataEmotionList;

        // Removes the last item in the list of moods
        data.RemoveAt(data.Count-1);

        // Adds the newest item to the list of emotions.
        data.Insert(0, emotion);

        _playerData.playerDataEmotionList = data;

        // Saves the playerdata that has been changed.
        StartCoroutine(SavePlayerData(_playerData, null));

        ClosePopup();
    }
}
