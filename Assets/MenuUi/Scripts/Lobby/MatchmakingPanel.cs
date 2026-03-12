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
            LobbyManager.OnGameStartCancelled += OnGameStartCancelled;
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
            LobbyManager.OnGameStartCancelled -= OnGameStartCancelled;
        }

        /// <summary>
        /// Set cancel button functionality in matchmaking panel.
        /// </summary>
        /// <param name="isLeader">If the client is leader of the matchmaking group.</param>
        public void SetCancelButton(bool isLeader)
        {
            if (_cancelButton == null) return;
            _cancelButton.onClick.RemoveAllListeners();

            if (isLeader)
            {
                _cancelButton.gameObject.SetActive(true);
                _cancelButton.interactable = true;
                var txt = _cancelButton.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = "Peruuta";
                _cancelButton.onClick.AddListener(() =>
                {
                    _cancelButton.interactable = false;
                    this.Publish(new LobbyManager.StopMatchmakingEvent(InLobbyController.SelectedGameType));
                    // If non-leader, close popup to reset UI state (except Clan2v2)
                    bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
                    if (!isLeader && InLobbyController.SelectedGameType != GameType.Clan2v2)
                    {
                        Signals.SignalBus.OnCloseBattlePopupRequestedSignal();
                    }
                });
            }
            else
            {
                // Use same stop-matchmaking behaviour for non-leaders to avoid invalid LeaveRoom calls
                _cancelButton.gameObject.SetActive(true);
                _cancelButton.interactable = true;
                var txt = _cancelButton.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = "Peruuta";
                _cancelButton.onClick.AddListener(() =>
                {
                    _cancelButton.interactable = false;
                    this.Publish(new LobbyManager.StopMatchmakingEvent(InLobbyController.SelectedGameType));
                });
            }
        }

        private void OnGameCountdownUpdate(int secondsRemaining)
        {
            if (_matchmakingText != null)
            {
                if (secondsRemaining > 0)
                {
                    _matchmakingText.text = $"Peli alkaa {secondsRemaining}...";
                }
                else if (secondsRemaining == 0)
                {
                    _matchmakingText.text = "Peli alkaa!";
                }
                else
                {
                    // negative sentinel -> ignore (handled by OnGameStartCancelled)
                }
            }
            if (_cancelButton != null)
            {
                // Only disable the cancel button when an active countdown (non-negative)
                if (secondsRemaining >= 0)
                {
                    _cancelButton.interactable = false;
                }
            }
        }

        private void OnGameStartCancelled()
        {
            if (_matchmakingText != null) _matchmakingText.text = "Etsitään peliä...";
            if (_cancelButton != null)
            {
                bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
                SetCancelButton(isLeader);
            }
        }

        private void OnFailedToStartMatchmakingGame()
        {
            PopupSignalBus.OnChangePopupInfoSignal("Virhe pelin aloittamisessa, lopetetaan pelin etsiminen.");
        }
    }
}
