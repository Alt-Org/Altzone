using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts;
using Altzone.Scripts.Config;

public class ClanPanel : MonoBehaviour
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
        _player = ServerManager.Instance.Player;

        if (_player == null)
            SetPlayerPanelValues(false);
        else
            SetPlayerPanelValues(true);

    }

    private void Reset()
    {
        _playerClanText.text = loggedOutClanText;
        _playerClanDescription.text = loggedOutClanDescription;
    }

    private void OnDisable()
    {
        ServerManager.OnLogInStatusChanged -= SetPlayerPanelValues;
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
            if (playerData.HasClanId)
            {
                clanId = playerData.ClanId;
                store.GetClanData(clanId, p => clanData = p);
                if (clanData != null)
                {
                    _playerClanText.text = clanData.Name;
                    _playerClanDescription.text = clanData.Phrase;
                }
                else
                {
                    _playerClanText.text = "Klaanin tietojen hakeminen ei onnistunut";
                    _playerClanDescription.text = "";
                }
            }
            else
            {
                _playerClanText.text = "Et ole klaanissa";
                _playerClanDescription.text = "Paina t�st� liitty�ksesi klaaniin";
            }
        }
        else
        {
            Reset();
        }
    }
}