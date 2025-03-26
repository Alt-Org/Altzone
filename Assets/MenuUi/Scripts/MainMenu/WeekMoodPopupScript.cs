using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Window;
using Quantum;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class WeekMoodPopupScript : AltMonoBehaviour
{
    [SerializeField] private GameObject _popupPrefab;
    [SerializeField] private UnityEngine.UI.Button _loveButton;
    [SerializeField] private UnityEngine.UI.Button _playfulButton;
    [SerializeField] private UnityEngine.UI.Button _joyButton;
    [SerializeField] private UnityEngine.UI.Button _sadButton;
    [SerializeField] private UnityEngine.UI.Button _angryButton;
    public enum Mood
    {
        Blank,
        Love,
        Playful,
        Joy,
        Sad,
        Angry
    }

    List<string> moodList = new List<string> { "Blank", "Love", "Playful", "Joy", "Sad", "Angry", "Blank" };

    string date = DateTime.Today.ToString("dd/mm/yyyy");

    public delegate void PlayerDataCallback(PlayerData playerData);
    public PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        _popupPrefab.SetActive(true);

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
    }

    

    public void ClosePopup()
    {
        _popupPrefab.SetActive(false);
    }

    void MoodToPlayerData(PlayerData mood)
    {

    }

    public void SaveMoodData(Mood mood)
    {
        /*

        moodList[6] = moodList[5];
        moodList[5] = moodList[4];
        moodList[4] = moodList[3];
        moodList[3] = moodList[2];
        moodList[2] = moodList[1];
        moodList[1] = moodList[0];

        switch (mood)
        {
            case Mood.Love:
                moodList[0] = Mood.Love;
                break;
            case Mood.Playful:
                moodList[0] = Mood.Playful;
                break;
            case Mood.Joy:
                moodList[0] = Mood.Joy;
                break;
            case Mood.Sad:
                moodList[0] = Mood.Sad;
                break;
            case Mood.Angry:
                moodList[0] = Mood.Angry;
                break;
        }*/
        
        

        Debug.Log("Mood saved: " + mood);

        //PlayerData.playerDataMoodList = moodList;
        //Debug.Log(playerData.playerDataMoodList[0]);
    }
}
