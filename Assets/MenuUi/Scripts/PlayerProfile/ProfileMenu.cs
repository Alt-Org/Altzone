using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MenuUi.Scripts.Window;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using Altzone.Scripts.Config;

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


    private ServerPlayer _player;

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
            //_TimePlayedText.text = ;
            //_LosesWinsText.text = ;
            //_CarbonText.text = ;
        }
        else
        {
            Reset();
        }
    }
}
