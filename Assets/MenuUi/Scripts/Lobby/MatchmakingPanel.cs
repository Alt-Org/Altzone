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
            // Ensure EventSystem exists
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                Debug.LogWarning("MatchmakingPanel: No EventSystem found in scene. UI clicks won't work without one.");
            }
            LobbyManager.OnFailedToStartMatchmakingGame += OnFailedToStartMatchmakingGame;
            LobbyManager.OnGameCountdownUpdate += OnGameCountdownUpdate;
            LobbyManager.OnGameStartCancelled += OnGameStartCancelled;
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
            // If inspector didn't assign the cancel button for this popup variant, try to find it in children.
            if (_cancelButton == null)
            {
                _cancelButton = GetComponentInChildren<Button>(true);
                if (_cancelButton == null)
                {
                    Debug.LogWarning("MatchmakingPanel: cancel button not assigned and none found in children.");
                    return;
                }
            }
            Debug.Log($"MatchmakingPanel.SetCancelButton: isLeader={isLeader}, button={_cancelButton.name}");
            _cancelButton.onClick.RemoveAllListeners();

            // Configure UI and listener via a single wrapper to aid debugging and ensure consistent behavior
            _cancelButton.gameObject.SetActive(true);
            _cancelButton.interactable = true;
            var txt = _cancelButton.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = "Peruuta";
            // Ensure the button (or its parent) allows raycasts
            var cg = _cancelButton.GetComponent<CanvasGroup>();
            if (cg != null) { cg.blocksRaycasts = true; cg.interactable = true; }
            // Bring button to front to avoid overlays blocking it
            try { _cancelButton.transform.SetAsLastSibling(); } catch { }

            // If there is a blocking overlay (common name: "Blocker") in the popup, disable its raycast targets
            try
            {
                Transform root = this.transform.root;
                Transform blockerT = null;
                if (root != null) blockerT = root.Find("Blocker");
                if (blockerT == null && this.transform.parent != null) blockerT = this.transform.parent.Find("Blocker");
                if (blockerT == null) blockerT = transform.Find("Blocker");
                if (blockerT != null)
                {
                    var blockerCg = blockerT.GetComponent<CanvasGroup>();
                    if (blockerCg != null) blockerCg.blocksRaycasts = false;
                    var blockerGraphic = blockerT.GetComponent<UnityEngine.UI.Graphic>();
                    if (blockerGraphic != null) blockerGraphic.raycastTarget = false;
                    var childGraphics = blockerT.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
                    foreach (var cgChild in childGraphics) cgChild.raycastTarget = false;
                }
            }
            catch { }

            // Ensure any parent CanvasGroup allows raycasts
            var t = _cancelButton.transform as Transform;
            while (t != null)
            {
                var parentCg = t.GetComponent<CanvasGroup>();
                if (parentCg != null)
                {
                    parentCg.blocksRaycasts = true;
                    parentCg.interactable = true;
                }
                t = t.parent;
            }

            // Ensure graphics under button accept raycasts
            var graphics = _cancelButton.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            foreach (var g in graphics) g.raycastTarget = true;

            _cancelButton.onClick.AddListener(OnCancelButtonPressed);
            // Add a Canvas on the button (or its parent) to ensure it renders above blockers and receives raycasts
            try
            {
                var btnRoot = _cancelButton.gameObject;
                var canvas = btnRoot.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = btnRoot.AddComponent<Canvas>();
                    // prefer parent canvas scale control; keep overrideSorting to bring it above others
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 5000;
                }
                else
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 5000;
                }
                if (btnRoot.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                {
                    btnRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
            }
            catch { }
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
            if (!isLeader && InLobbyController.SelectedGameType != GameType.Clan2v2 && InLobbyController.SelectedGameType != GameType.InRoom_)
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
