using Altzone.Scripts.Lobby;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Lobby.InLobby;
using MenuUi.Scripts.Signals;
using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.Lobby
{
    /// <summary>
    /// Small matchmaking overlay panel shown on UIOverlayPanel when the battle popup is closed during matchmaking.
    /// Inversely active with the battle popup: visible when matchmaking is active and the battle popup is closed.
    /// </summary>
    public class MiniMatchmakingPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private GameObject _battlePopup;
        [SerializeField] private TMP_Text _matchmakingText;
        [SerializeField] private TMP_Text _elapsedTimeText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _panelButton;

        private bool _isMatchmaking;
        private float _matchmakingStartTime;
        private RectTransform _rectTransform;
        private Vector2 _savedAnchorMin;
        private Vector2 _savedAnchorMax;
        private Vector2 _savedOffsetMin;
        private Vector2 _savedOffsetMax;

        private void Awake()
        {
            // Save the prefab anchors before ChildAnchorSetter can overwrite them
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _savedAnchorMin = _rectTransform.anchorMin;
                _savedAnchorMax = _rectTransform.anchorMax;
                _savedOffsetMin = _rectTransform.offsetMin;
                _savedOffsetMax = _rectTransform.offsetMax;
            }

            LobbyManager.OnMatchmakingRoomEntered += OnMatchmakingRoomEntered;
            LobbyManager.OnMatchmakingStopped += OnMatchmakingStopped;
            LobbyManager.OnGameCountdownUpdate += OnGameCountdownUpdate;
            LobbyManager.OnGameStartCancelled += OnGameStartCancelled;
            LobbyManager.OnRoomLeaderChanged += SetCancelButton;
            LobbyManager.OnFailedToStartMatchmakingGame += OnFailedToStartMatchmakingGame;

            // Subscribe to runtime popup registration so we can get the popup GameObject when views load later
            InLobbyController.OnPopupContentsInstanceAssigned += OnPopupContentsRegistered;

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }

            if (_panelButton != null)
            {
                _panelButton.onClick.AddListener(OnPanelClicked);
            }

            // If not assigned in inspector, try to grab the popup contents instance from InLobbyController (runtime)
            if (_battlePopup == null && InLobbyController.PopupContentsInstance != null)
            {
                _battlePopup = InLobbyController.PopupContentsInstance; 
            }

            SetVisible(false);

            // Initialize state from current Photon/room state in case matchmaking started before this component was enabled
            TryInitFromCurrentState();
        }

        private void TryInitFromCurrentState()
        {
            try
            {
                if (PhotonRealtimeClient.InMatchmakingRoom)
                {
                    bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;

                    _isMatchmaking = true;
                    _matchmakingStartTime = Time.time;
                    SetCancelButton(isLeader);
                }
            }
            catch
            {
                // Photon client not ready yet — ignore and rely on events/fallbacks
            }
            UpdateVisibility();
        }

        private void OnEnable()
        {
            // Restore anchors in case ChildAnchorSetter overwrote them
            if (_rectTransform != null)
            {
                _rectTransform.anchorMin = _savedAnchorMin;
                _rectTransform.anchorMax = _savedAnchorMax;
                _rectTransform.offsetMin = _savedOffsetMin;
                _rectTransform.offsetMax = _savedOffsetMax;
            }
        }

        private void OnDestroy()
        {
            LobbyManager.OnMatchmakingRoomEntered -= OnMatchmakingRoomEntered;
            LobbyManager.OnMatchmakingStopped -= OnMatchmakingStopped;
            LobbyManager.OnGameCountdownUpdate -= OnGameCountdownUpdate;
            LobbyManager.OnGameStartCancelled -= OnGameStartCancelled;
            LobbyManager.OnRoomLeaderChanged -= SetCancelButton;
            LobbyManager.OnFailedToStartMatchmakingGame -= OnFailedToStartMatchmakingGame;

            if (_cancelButton != null) _cancelButton.onClick.RemoveAllListeners();
            if (_panelButton != null) _panelButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            UpdateVisibility();

            if (_isMatchmaking && _elapsedTimeText != null)
            {
                float elapsed = Time.time - _matchmakingStartTime;
                int minutes = (int)(elapsed / 60f);
                int seconds = (int)(elapsed % 60f);
                _elapsedTimeText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        private void OnPopupContentsRegistered(GameObject popup)
        {
            if (popup != null && _battlePopup == null)
            {
                _battlePopup = popup;
                UpdateVisibility();
            }
        }

        /// <summary>
        /// Show the mini panel. Called externally when the battle popup is closed during matchmaking.
        /// </summary>
        public void Show()
        {
            UpdateVisibility();
        }

        /// <summary>
        /// Hide the mini panel. Called externally when the battle popup is opened.
        /// </summary>
        public void Hide()
        {
            UpdateVisibility();
        }

        private void SetVisible(bool visible)
        {
            if (_content != null) _content.SetActive(visible);
        }

        private void UpdateVisibility()
        {
            bool battlePopupActive = _battlePopup != null && _battlePopup.activeInHierarchy;
            SetVisible(_isMatchmaking && !battlePopupActive);
        }

        private void OnMatchmakingRoomEntered(bool isLeader)
        {
            _isMatchmaking = true;
            _matchmakingStartTime = Time.time;

            if (_matchmakingText != null)
            {
                _matchmakingText.text = "Etsitään peliä...";
            }

            SetCancelButton(isLeader);
            UpdateVisibility();
        }

        private void OnMatchmakingStopped()
        {
            _isMatchmaking = false;
            UpdateVisibility();
        }

        private void SetCancelButton(bool isLeader)
        {
            if (_cancelButton == null) return;
            _cancelButton.onClick.RemoveAllListeners();

            if (isLeader)
            {
                _cancelButton.interactable = true;
                _cancelButton.gameObject.SetActive(true);
                var txt = _cancelButton.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = "Peruuta";
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }
            else
            {
                // Show Leave button for non-leaders so they can exit without opening the popup
                _cancelButton.gameObject.SetActive(true);
                _cancelButton.interactable = true;
                var txt = _cancelButton.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = "Poistu";
                _cancelButton.onClick.AddListener(() =>
                {
                    PhotonRealtimeClient.LeaveRoom();
                    if (InLobbyController.SelectedGameType != GameType.Clan2v2) Signals.SignalBus.OnCloseBattlePopupRequestedSignal();
                });
            }
        }

        private void OnCancelClicked()
        {
            if (_cancelButton != null) _cancelButton.interactable = false;
            this.Publish(new LobbyManager.StopMatchmakingEvent(InLobbyController.SelectedGameType));
        }

        private void OnPanelClicked()
        {
            // Reopen the battle popup and hide this mini panel
            Hide();
            Signals.SignalBus.OnBattlePopupRequestedSignal(InLobbyController.SelectedGameType);
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
                _cancelButton.interactable = false;
            }
        }

        private void OnGameStartCancelled()
        {
            _isMatchmaking = true;
            _matchmakingStartTime = Time.time;
            if (_matchmakingText != null) _matchmakingText.text = "Etsitään peliä...";
            bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
            SetCancelButton(isLeader);
            UpdateVisibility();
        }

        private void OnFailedToStartMatchmakingGame()
        {
            _isMatchmaking = false;
            UpdateVisibility();
        }
    }
}
