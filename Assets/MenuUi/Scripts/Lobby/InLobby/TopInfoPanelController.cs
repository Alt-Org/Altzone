using UnityEngine;
using TMPro;

namespace MenuUi.Scripts.Lobby.InLobby
{
    /// <summary>
    /// Handles setting visual info to the top info panel in Battle Popup.
    /// </summary>
    public class TopInfoPanelController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _lobbyText;
        [SerializeField] private TextMeshProUGUI _playerCountText;
        
        public string TitleText
        {
            set => _titleText.text = value;
        }

        public string LobbyText
        {
            set => _lobbyText.text = value;
        }
        public string PlayerCountText
        {
            set => _playerCountText.text = value;
        }

        public void Reset()
        {
            _titleText.text = string.Empty;
            _lobbyText.text = string.Empty;
        }
    }
}
