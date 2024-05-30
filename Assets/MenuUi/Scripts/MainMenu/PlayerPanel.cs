using UnityEngine;
using TMPro;
using MenuUi.Scripts.Window;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using Altzone.Scripts.Config;

public class PlayerPanel : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string loggedOutPlayerText;
    [SerializeField] private string loggedOutClanText;
    [SerializeField] private string loggedOutClanDescription;

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerClanText;
    [SerializeField] private TextMeshProUGUI _playerClanDescription;

    [Header("Navigation Buttons")]
    [SerializeField] private NaviButton _profileNaviButton;

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
        _playerNameText.text = loggedOutPlayerText;
        _playerClanText.text = loggedOutClanText;
        _playerClanDescription.text = loggedOutClanDescription;
        _profileNaviButton.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ServerManager.OnLogInStatusChanged -= SetPlayerPanelValues;
    }

    /// <summary>
    /// Sets the name text and clan text of player panel when log in status changes
    /// </summary>
    /// <param name="isLoggedIn">Logged in status</param>
    private void SetPlayerPanelValues(bool isLoggedIn)
    {
        if(isLoggedIn)
        {
            // Gets the player name from DataStorage
            var store = Storefront.Get();
            PlayerData playerData = null;
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
            _playerNameText.text = playerData.Name;
            _playerClanText.text = "Tervetuloa ALT Zoneen!";
            _playerClanDescription.text = "";

            _profileNaviButton.gameObject.SetActive(true);
        }
        else
        {
            Reset();
        }
    }
}
