using System;
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
        [SerializeField] private TextLanguageSelectorCaller _matchmakingCountText;
        private float _updateInterval = 0.5f;
        private float _lastUpdate = 0f;
        
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
        public string MatchmakingCountText
        {
            set => _matchmakingCountText.SetText(SettingsCarrier.Instance.Language, new string[1] { value });
        }

        public void Reset()
        {
            _titleText.SetText(string.Empty);
            _lobbyText.SetText(string.Empty);
        }

        private void Update()
        {
            if (Time.time - _lastUpdate < _updateInterval) return;
            _lastUpdate = Time.time;
            try
            {
                // Keep title/version and counts refreshed
                _titleText.SetText($"{Application.productName} {PhotonRealtimeClient.GameVersion}");
                var region = PhotonRealtimeClient.CloudRegion ?? string.Empty;
                _lobbyText.SetText(SettingsCarrier.Instance.Language, new string[2] { region, PhotonRealtimeClient.GetPing().ToString() });
                _playerCountText.SetText(SettingsCarrier.Instance.Language, new string[1] { PhotonRealtimeClient.CountOfPlayers.ToString() });
                _matchmakingCountText.SetText(SettingsCarrier.Instance.Language, new string[1] { PhotonRealtimeClient.CurrentRoomPlayerCount.ToString() });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"TopInfoPanelController.Update failed: {ex.Message}");
            }
        }
    }
}
