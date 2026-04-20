using System.Security.Claims;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using TMPro;
using UnityEngine;

public class TopNamePanel : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string loggedOutClanText;
    [SerializeField] private string loggedOutClanDescription;

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI _playerClanText;
    [SerializeField] private TextMeshProUGUI _playerClanDescription;

    [Header("Navigation Buttons")]
    [SerializeField] private GameObject _clanButtonGameObject;

    private ServerPlayer _player;

    private void OnEnable()
    {
        ServerManager.OnLogInStatusChanged += SetPlayerPanelValues;
        ServerManager.OnClanChanged += SetClanValues;
        _player = ServerManager.Instance.Player;

        if (_player == null)
            SetPlayerPanelValues(false);
        else
            SetPlayerPanelValues(true);

    }

    private void Reset()
    {
        if (_playerClanText != null) _playerClanText.text = loggedOutClanText;
        if (_playerClanDescription != null) _playerClanDescription.text = loggedOutClanDescription;
    }

    private void OnDisable()
    {
        ServerManager.OnLogInStatusChanged -= SetPlayerPanelValues;
        ServerManager.OnClanChanged -= SetClanValues;
;
}

    /// <summary>
    /// Sets the clan text of clan panel when log in status changes.
    /// </summary>
    /// <param name="isLoggedIn">Logged in status</param>
    private void SetPlayerPanelValues(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            // Gets the player data from DataStorage
            var store = Storefront.Get();
            PlayerData playerData = null;
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);

            ClanData clanData = null;
            string clanId = "";
            if(playerData == null)
            {
                _playerClanText.text = "Et ole kirjautunut sisään";
                _playerClanDescription.text = "";
            }
            if (playerData.HasClanId)
            {
                clanId = playerData.ClanId;
                store.GetClanData(clanId, p => clanData = p);
                if (clanData != null)
                {
                    if(_playerClanText != null) _playerClanText.text = clanData.Name;
                    if (_playerClanDescription != null) _playerClanDescription.text = playerData.Name;
                }
                else
                {
                    if (_playerClanText != null) _playerClanText.text = "Klaanin tietojen hakeminen ei onnistunut";
                    if (_playerClanDescription != null) _playerClanDescription.text = playerData.Name;
                }
            }
            else
            {
                if (_playerClanText != null) _playerClanText.text = "Et ole klaanissa";
                if (_playerClanDescription != null) _playerClanDescription.text = playerData.Name;
            }
        }
        else
        {
            Reset();
        }
    }

    private void SetClanValues(ServerClan Clan)
    {
        ClanData clanData = null;
        string clanId = "";
        if (Clan != null) clanData = new(Clan);
        if (clanData != null)
        {
            clanId = clanData.Id;
            if (clanData != null)
            {
                if (_playerClanText != null) _playerClanText.text = clanData.Name;
            }
            else
            {
                if (_playerClanText != null) _playerClanText.text = "Klaanin tietojen hakeminen ei onnistunut";
            }
        }
        else
        {
            if (_playerClanText != null) _playerClanText.text = "Et ole klaanissa";
        }
    }
}
