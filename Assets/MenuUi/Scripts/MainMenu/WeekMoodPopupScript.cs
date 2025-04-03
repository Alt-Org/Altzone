using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Window;
using Quantum;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class WeekMoodPopupScript : AltMonoBehaviour
{
    // The popup that has all the mood options.
    [SerializeField] private GameObject _popupPrefab;

    // The mood buttons that are in the popup.
    [SerializeField] private UnityEngine.UI.Button _loveButton;
    [SerializeField] private UnityEngine.UI.Button _playfulButton;
    [SerializeField] private UnityEngine.UI.Button _joyButton;
    [SerializeField] private UnityEngine.UI.Button _sadButton;
    [SerializeField] private UnityEngine.UI.Button _angryButton;

    // Moods
    public enum Mood
    {
        Blank,
        Love,
        Playful,
        Joy,
        Sad,
        Angry
    }

    // Creates the variable that is used to get the list of the moods.
    private PlayerData _playerData;

    // Variable which callbacks.
    private readonly Action<PlayerData> _callback;

    // Start is called before the first frame update
    void Start()
    {
        // Opens the popup.
        _popupPrefab.SetActive(true);

        // Gets the needed playerdata
        StartCoroutine(GetPlayerData(data => _playerData = data));

        // Listeners that listen what button has been pressed and does the method given.
        // The buttons have their own mood so its easier to add the mood to the list.
        _loveButton.onClick.AddListener(() => SaveMoodData(Mood.Love));
        _playfulButton.onClick.AddListener(() => SaveMoodData(Mood.Playful));
        _joyButton.onClick.AddListener(() => SaveMoodData(Mood.Joy));
        _sadButton.onClick.AddListener(() => SaveMoodData(Mood.Sad));
        _angryButton.onClick.AddListener(() => SaveMoodData(Mood.Angry));

        _loveButton.onClick.AddListener(() => ClosePopup());
        _playfulButton.onClick.AddListener(() => ClosePopup());
        _joyButton.onClick.AddListener(() => ClosePopup());
        _sadButton.onClick.AddListener(() => ClosePopup());
        _angryButton.onClick.AddListener(() => ClosePopup());

        // Saves the playerdata that has been changed.
        Storefront.Get().SavePlayerData(_playerData, _callback);
    }


    // Closes the popup.
    public void ClosePopup()
    {
        _popupPrefab.SetActive(false);
    }

    // Saves the mood that the player has chosen.
    public void SaveMoodData(Mood mood)
    {
        // Removes the last item in the list of moods
        _playerData.playerDataMoodList.RemoveAt(6);

        // Reverses the list so the newest mood can get in to first place. last-newest
        _playerData.playerDataMoodList.Reverse();

        // Adds the newest mood to the list.
        _playerData.playerDataMoodList.Add(mood);
        
        // Reverses the list back the way we want it. newest-last
        _playerData.playerDataMoodList.Reverse();
    }
}
