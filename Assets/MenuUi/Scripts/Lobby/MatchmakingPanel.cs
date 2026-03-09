using Altzone.Scripts.Lobby;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Lobby.InLobby;
using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.Lobby
{
    /// <summary>
    /// Handles matchmaking panel functionality in battle popup.
    /// </summary>
    public class MatchmakingPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _matchmakingText;
        [SerializeField] private Button _cancelButton;

        private void Awake()
        {
            LobbyManager.OnFailedToStartMatchmakingGame += OnFailedToStartMatchmakingGame;
            LobbyManager.OnGameCountdownUpdate += OnGameCountdownUpdate;
        }

        private void OnEnable()
        {
            LobbyManager.OnRoomLeaderChanged += SetCancelButton;
        }

        private void OnDisable()
        {
            LobbyManager.OnRoomLeaderChanged -= SetCancelButton;
        }

        private void OnDestroy()
        {
            _cancelButton.onClick.RemoveAllListeners();
            LobbyManager.OnFailedToStartMatchmakingGame -= OnFailedToStartMatchmakingGame;
            LobbyManager.OnGameCountdownUpdate -= OnGameCountdownUpdate;
        }

        /// <summary>
        /// Set cancel button functionality in matchmaking panel.
        /// </summary>
        /// <param name="isLeader">If the client is leader of the matchmaking group.</param>
        public void SetCancelButton(bool isLeader)
        {
            if (isLeader)
            {
                _cancelButton.interactable = true;
                _cancelButton.gameObject.SetActive(true);
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(() =>
                {
                    _cancelButton.interactable = false;
                    this.Publish(new LobbyManager.StopMatchmakingEvent(InLobbyController.SelectedGameType));
                });
            }
            else
            {
                _cancelButton.gameObject.SetActive(false);
            }
        }

        private void OnGameCountdownUpdate(int secondsRemaining)
        {
            if (_matchmakingText != null)
            {
                _matchmakingText.text = secondsRemaining > 0
                    ? $"Peli alkaa {secondsRemaining}..."
                    : "Peli alkaa!";
            }
            if (_cancelButton != null)
            {
                _cancelButton.interactable = false;
            }
        }

        private void OnFailedToStartMatchmakingGame()
        {
            PopupSignalBus.OnChangePopupInfoSignal("Virhe pelin aloittamisessa, lopetetaan pelin etsiminen.");
        }
    }
}
