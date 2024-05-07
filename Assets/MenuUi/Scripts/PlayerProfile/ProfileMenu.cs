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

public class ProfileMenu : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string loggedOutPlayerText;
    [SerializeField] private string loggedOutTimeText;
    [SerializeField] private string loggedOutLosesWinsText;
    [SerializeField] private string loggedOutCarbonText;

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _TimePlayedText;
    [SerializeField] private TextMeshProUGUI _LosesWinsText;
    [SerializeField] private TextMeshProUGUI _CarbonText;

    //[Header("Images")]
    // [SerializeField] private GameObject _BattleCharacter = new GameObject();

    // private Sprite Img;

    private int hourCount;
    private int minuteCount;
    private float secondsCount;
    private float cabrbonCount;

    private ServerPlayer _player;

    private void Update()
    {
        updateTime();
    }

    private void updateTime()
    {
        secondsCount += Time.deltaTime;
        _TimePlayedText.text = hourCount.ToString() + " " + minuteCount.ToString() + " " + secondsCount.ToString("0");
        _CarbonText.text = cabrbonCount.ToString();
        if (secondsCount > 60)
        {
            minuteCount++;
            cabrbonCount = cabrbonCount + 1.5f;
            secondsCount = 0;
        }
        else if (minuteCount > 60)
        {
            hourCount++;
            minuteCount = 0;
        }

    }

    private void OnEnable()
    {
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
