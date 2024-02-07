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

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerClanText;

    [Header("Navigation Buttons")]
    [SerializeField] private NaviButton _signInNaviButton;
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
        _signInNaviButton.gameObject.SetActive(true);
        _profileNaviButton.gameObject.SetActive(false);
        _clanButtonGameObject.SetActive(false);
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
            _playerNameText.text = "Hei " + playerData.Name + "!";
            _playerClanText.text = "Tervetuloa ALT Zoneen!";

            _signInNaviButton.gameObject.SetActive(false);
            _profileNaviButton.gameObject.SetActive(true);
            _clanButtonGameObject.SetActive(true);
        }
        else
        {
            Reset();
        }
    }
}
