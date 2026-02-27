using UnityEngine;
using TMPro;
using Altzone.Scripts.Language;

namespace MenuUi.Scripts.Lobby.InLobby
{
    /// <summary>
    /// Handles setting visual info to the top info panel in Battle Popup.
    /// </summary>
    public class TopInfoPanelController : MonoBehaviour
    {
        [SerializeField] private TextLanguageSelectorCaller _titleText;
        [SerializeField] private TextLanguageSelectorCaller _lobbyText;
        [SerializeField] private TextLanguageSelectorCaller _playerCountText;
        
        public string TitleText
        {
            set => _titleText.SetText(value);
        }

        public string LobbyTextLiteral
        {
            set => _lobbyText.SetText(value);
        }
        public string[] LobbyText
        {
            set => _lobbyText.SetText(SettingsCarrier.Instance.Language, value);
        }
        public string PlayerCountText
        {
            set => _playerCountText.SetText(SettingsCarrier.Instance.Language, new string[1] { value });
        }

        public void Reset()
        {
            _titleText.SetText(string.Empty);
            _lobbyText.SetText(string.Empty);
        }
    }
}
