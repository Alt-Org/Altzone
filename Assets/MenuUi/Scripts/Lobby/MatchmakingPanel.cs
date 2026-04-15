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
        [SerializeField] private TMP_Text _cancelButtonText;

        private void Awake()
        {
            ValidateSerializedFields();

            // Ensure EventSystem exists
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                Debug.LogWarning("MatchmakingPanel: No EventSystem found in scene. UI clicks won't work without one.");
            }
            LobbyManager.OnFailedToStartMatchmakingGame += OnFailedToStartMatchmakingGame;
            LobbyManager.OnGameCountdownUpdate += OnGameCountdownUpdate;
            LobbyManager.OnGameStartCancelled += OnGameStartCancelled;
        }

        private void ValidateSerializedFields()
        {
            if (_matchmakingText == null)
            {
                throw new MissingReferenceException($"{nameof(MatchmakingPanel)} on '{name}' is missing reference: {nameof(_matchmakingText)}. Assign it in prefab.");
            }

            if (_cancelButton == null)
            {
                throw new MissingReferenceException($"{nameof(MatchmakingPanel)} on '{name}' is missing reference: {nameof(_cancelButton)}. Assign it in prefab.");
            }

            if (_cancelButtonText == null)
            {
                throw new MissingReferenceException($"{nameof(MatchmakingPanel)} on '{name}' is missing reference: {nameof(_cancelButtonText)}. Assign it in prefab.");
            }
        }

        private void OnEnable()
        {
            LobbyManager.OnRoomLeaderChanged += SetCancelButton;
            LobbyManager.OnMatchmakingRoomEntered += OnMatchmakingRoomEntered;
            LobbyManager.OnMatchmakingStopped += OnMatchmakingStopped;

            SetMatchmakingTextForCurrentRoom();
            bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
            SetCancelButton(isLeader);
        }

        private void OnDisable()
        {
            LobbyManager.OnRoomLeaderChanged -= SetCancelButton;
            LobbyManager.OnMatchmakingRoomEntered -= OnMatchmakingRoomEntered;
            LobbyManager.OnMatchmakingStopped -= OnMatchmakingStopped;
        }

        private void OnDestroy()
        {
            if (_cancelButton != null) _cancelButton.onClick.RemoveAllListeners();
            LobbyManager.OnFailedToStartMatchmakingGame -= OnFailedToStartMatchmakingGame;
            LobbyManager.OnGameCountdownUpdate -= OnGameCountdownUpdate;
            LobbyManager.OnGameStartCancelled -= OnGameStartCancelled;
            LobbyManager.OnMatchmakingRoomEntered -= OnMatchmakingRoomEntered;
            LobbyManager.OnMatchmakingStopped -= OnMatchmakingStopped;
        }

        /// <summary>
        /// Set cancel button functionality in matchmaking panel.
        /// </summary>
        /// <param name="isLeader">If the client is leader of the matchmaking group.</param>
        public void SetCancelButton(bool isLeader)
        {
            Debug.Log($"MatchmakingPanel.SetCancelButton: isLeader={isLeader}, button={_cancelButton.name}");
            _cancelButton.onClick.RemoveAllListeners();

            // Configure UI and listener; popup ordering should be handled in prefab/canvas setup.
            _cancelButton.gameObject.SetActive(true);
            _cancelButton.interactable = true;
            _cancelButtonText.text = "Peruuta";
            var cg = _cancelButton.GetComponent<CanvasGroup>();
            if (cg != null) { cg.blocksRaycasts = true; cg.interactable = true; }

            // Ensure graphics under button accept raycasts
            var graphics = _cancelButton.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics) g.raycastTarget = true;

            _cancelButton.onClick.AddListener(OnCancelButtonPressed);
            // If caller is leader and extra behavior is needed, OnCancelButtonPressed will handle it based on current master state
        }

        private void OnCancelButtonPressed()
        {
            Debug.Log("MatchmakingPanel: Cancel button pressed (OnCancelButtonPressed)");
            if (_cancelButton != null) _cancelButton.interactable = false;
            try
            {
                var lobbyRoom = PhotonRealtimeClient.LobbyCurrentRoom;
                if (lobbyRoom != null && lobbyRoom.GetCustomProperty<bool>(Altzone.Scripts.Battle.Photon.PhotonBattleRoom.IsQueueKey, false))
                {
                    Debug.Log("MatchmakingPanel: leaving queue room immediately due to user cancel.");
                    PhotonRealtimeClient.LeaveRoom();
                }
            }
            catch { }

            this.Publish(new LobbyManager.StopMatchmakingEvent(InLobbyController.SelectedGameType, true));
            bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
            if (!isLeader && InLobbyController.SelectedGameType != GameType.Clan2v2)
            {
                Signals.SignalBus.OnCloseBattlePopupRequestedSignal();
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
            SetMatchmakingTextForCurrentRoom();
            if (_cancelButton != null)
            {
                bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
                SetCancelButton(isLeader);
            }
        }

        private void OnMatchmakingRoomEntered(bool isLeader)
        {
            SetMatchmakingTextForCurrentRoom();
            SetCancelButton(isLeader);
        }

        private void OnMatchmakingStopped()
        {
            if (_matchmakingText != null) _matchmakingText.text = "";
        }

        private void SetMatchmakingTextForCurrentRoom()
        {
            if (_matchmakingText == null) return;

            try
            {
                bool inMatchmakingOrQueue = PhotonRealtimeClient.InMatchmakingRoom;

                var room = PhotonRealtimeClient.LobbyCurrentRoom;
                bool isQueue = false;
                if (room != null)
                {
                    isQueue = room.GetCustomProperty<bool>(Altzone.Scripts.Battle.Photon.PhotonBattleRoom.IsQueueKey);
                    if (isQueue) inMatchmakingOrQueue = true;
                }

                if (!inMatchmakingOrQueue)
                {
                    _matchmakingText.text = string.Empty;
                    return;
                }

                _matchmakingText.text = isQueue ? "Jonossa..." : "Etsitään peliä...";
            }
            catch
            {
                _matchmakingText.text = "Etsitään peliä...";
            }
        }

        private void OnFailedToStartMatchmakingGame()
        {
            PopupSignalBus.OnChangePopupInfoSignal("Virhe pelin aloittamisessa, lopetetaan pelin etsiminen.");
        }
    }
}
