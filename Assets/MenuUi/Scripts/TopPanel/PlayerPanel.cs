using UnityEngine;
using TMPro;
using MenuUi.Scripts.Window;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts;
using Altzone.Scripts.Config;

public class PlayerPanel : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string loggedOutPlayerText;

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;

    [Header("Navigation Buttons")]
    [SerializeField] private NaviButton _profileNaviButton;

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
        _profileNaviButton.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ServerManager.OnLogInStatusChanged -= SetPlayerPanelValues;
    }

    /// <summary>
    /// Sets the name text of player panel when log in status changes
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
            _profileNaviButton.gameObject.SetActive(true);
        }
        else
        {
            Reset();
        }
    }
}
