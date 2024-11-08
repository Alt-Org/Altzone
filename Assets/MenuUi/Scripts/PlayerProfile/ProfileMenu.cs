using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MenuUi.Scripts.Window;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.UIElements;

public class ProfileMenu : MonoBehaviour

{


    [Header("Text")]
    [SerializeField] private string loggedOutPlayerText;
    [SerializeField] private string loggedOutTimeText;
    [SerializeField] private string loggedOutLosesWinsText;
    [SerializeField] private string loggedOutCarbonText;

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _MottoText;
    [SerializeField] private TextMeshProUGUI _TimePlayedText;
    [SerializeField] private TextMeshProUGUI _LosesWinsText;
    [SerializeField] private TextMeshProUGUI _CarbonText;



    //[Header("Images")]
    // [SerializeField] private GameObject _BattleCharacter = new GameObject();

    // private Sprite Img;

    private int tempLocalSaveTime;
    private float tempLocalSaveSecondsTime;
    private float tempLocalSaveToCarbon;
    private float tempLocalSaveCarbon;


    private int minuteCount;
    private float secondsCount;
    private float countToCarbon;
    private float carbonCount;


    private float kgCarbon => CarbonFootprint.CarbonCount / 1000f;

    

    private ServerPlayer _player;

    private void Update()
    {
        updateTime();
    }


    private void updateTime()
    {
        secondsCount += Time.deltaTime;
        countToCarbon += Time.deltaTime;

        // Peliaika
        if (minuteCount < 1)
        {
            _TimePlayedText.text = $"Peliaika\nAlle 1 min";
        }

        else
        {
            _TimePlayedText.text = $"Peliaika\n{minuteCount} min";
        }



        // Tarkistaa onko kg vai g
        float carbonDisplay = CarbonFootprint.CarbonCount;
        string carbonUnit = "g";

        if (carbonDisplay >= 1000f)                                                                                  
        {
            carbonDisplay /= 1000f;
            carbonUnit = "kg";
        }

        _CarbonText.text = $"Hiilijalanjälki\n{carbonDisplay:F1}{carbonUnit}/CO2"; // Hiilijalanjälki teksti

        // Päivittää minuutin välein peliajan.
        if (secondsCount >= 60f)
        {
            minuteCount++;
            secondsCount = 0;
        }
    }

    private void OnEnable()
    {
        //tempLocalSaveTime
        ServerManager.OnLogInStatusChanged += SetPlayerProfileValues;
        _player = ServerManager.Instance.Player;

        if (_player == null)
            SetPlayerProfileValues(false);
        else
            SetPlayerProfileValues(true);
    }

    private void Reset()
    {
        _playerNameText.text = loggedOutPlayerText;
        _TimePlayedText.text = loggedOutTimeText;
        _LosesWinsText.text = loggedOutLosesWinsText;
        _CarbonText.text = loggedOutCarbonText;
    }

    /// <summary>
    /// Sets the name of the Player and stats when log in status changes
    /// </summary>
    /// <param name="isLoggedIn">Logged in status</param>
    private void SetPlayerProfileValues(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            // Gets the player name from DataStorage
            var store = Storefront.Get();
            PlayerData playerData = null;
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
            _playerNameText.text = playerData.Name;
            //_BattleCharacter.AddComponent(typeof(Image));
            //Img = Resources.Load<Sprite>(playerData.BattleCharacter.Name);
            //_BattleCharacter.GetComponent<Image>().sprite = Img;
            updateTime();
            //_LosesWinsText.text = ;
        }
        else
        {
            Reset();
        }
    }


}

