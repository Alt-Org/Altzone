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

public class ProfileMenu : MonoBehaviour

    // Luokka siis näyttää pelaaja profiili statit (pelitunnit[mitkä tällä hetkellä näyttää ne minuutteina], voitot/häviöt ja pelaajan hiilijalanjäljen)
    // Hiilijalanjälkilaskuri pääosin toimii oletusarvojen mukaan, mutta jos on Android-laitteella, se yrittää etsiä tiedot virrankulutukseen AINAKIN (30.10.2024) -Eemeli
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
        carbonCount = CarbonFootprint.CarbonCount;
        _TimePlayedText.text = "Peliaika\n" + minuteCount.ToString();

        if (CarbonFootprint.CarbonCount >= 1000f)
        {
            _CarbonText.text = $"Hiilijalanjälki\n{kgCarbon:F2}kg/CO2";
        } else
        {
            _CarbonText.text = $"Hiilijalanjälki\n{carbonCount:F1}g/CO2";
        }
        

        if (secondsCount > 60)
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

