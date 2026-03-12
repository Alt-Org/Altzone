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
        [SerializeField] private TMP_Text _matchmakingCountText;
        [SerializeField] private TMP_Text _elapsedTimeText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _panelButton;

        private bool _isMatchmaking;
        private bool _isBattleStarting;
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
            LobbyManager.OnStartTimeSet += OnStartTimeSet;
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

            // Try to auto-assign matchmaking count text if inspector field is empty
            TryFindMatchmakingCountText();

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
            LobbyManager.OnStartTimeSet -= OnStartTimeSet;
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

            if (_isMatchmaking && _matchmakingCountText != null)
            {
                int count = PhotonRealtimeClient.CurrentRoomPlayerCount;
                _matchmakingCountText.text = count.ToString();
            }
        }

        private void OnPopupContentsRegistered(GameObject popup)
        {
            if (popup != null && _battlePopup == null)
            {
                _battlePopup = popup;
                UpdateCancelButtonFromState();
                TryFindMatchmakingCountText();
                UpdateVisibility();
            }
        }

        private void TryFindMatchmakingCountText()
        {
            if (_matchmakingCountText != null) return;
            Transform root = _content != null ? _content.transform : transform;
            var texts = root.GetComponentsInChildren<TMP_Text>(true);
            TMP_Text candidate = null;
            foreach (var t in texts)
            {
                if (t == _matchmakingText || t == _elapsedTimeText) continue;
                string n = t.gameObject.name.ToLowerInvariant();
                if (n.Contains("count") || n.Contains("matchmaking") || n.Contains("players") || n.Contains("match"))
                {
                    candidate = t;
                    break;
                }
            }
            if (candidate == null && texts.Length > 0)
            {
                foreach (var t in texts)
                {
                    if (t != _matchmakingText && t != _elapsedTimeText)
                    {
                        candidate = t;
                        break;
                    }
                }
            }
            if (candidate != null)
            {
                _matchmakingCountText = candidate;
                candidate.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Show the mini panel. Called externally when the battle popup is closed during matchmaking.
        /// </summary>
        public void Show()
        {
            UpdateCancelButtonFromState();
            UpdateVisibility();
        }

        private void UpdateCancelButtonFromState()
        {
            if (_cancelButton == null) return;
            bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
            SetCancelButton(isLeader);
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
            SetVisible(_isMatchmaking && !_isBattleStarting && !battlePopupActive);
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
            _isBattleStarting = false;
            UpdateVisibility();
        }

        private void SetCancelButton(bool isLeader)
        {
            if (_cancelButton == null) return;
            _cancelButton.onClick.RemoveAllListeners();

            // Use the same behaviour for all clients: stop matchmaking (Peruuta)
            _cancelButton.gameObject.SetActive(true);
            _cancelButton.interactable = true;
            var txt = _cancelButton.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = "Peruuta";
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnCancelClicked()
        {
            if (_cancelButton != null) _cancelButton.interactable = false;
            // Hide locally immediately so UI feedback is instant for the clicking user
            _isMatchmaking = false;
            _isBattleStarting = false;
            SetVisible(false);
            this.Publish(new LobbyManager.StopMatchmakingEvent(InLobbyController.SelectedGameType));
            // If caller is non-leader, close the battle popup to reset UI state (except Clan2v2)
            bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
            if (!isLeader && InLobbyController.SelectedGameType != GameType.Clan2v2)
            {
                Signals.SignalBus.OnCloseBattlePopupRequestedSignal();
            }
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
                // Only disable the cancel button when an active countdown (non-negative)
                if (secondsRemaining >= 0)
                {
                    _cancelButton.interactable = false;
                }
            }
        }

        private void OnGameStartCancelled()
        {
            _isMatchmaking = true;
            _isBattleStarting = false;
            _matchmakingStartTime = Time.time;
            if (_matchmakingText != null) _matchmakingText.text = "Etsitään peliä...";
            bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
            SetCancelButton(isLeader);
            UpdateVisibility();
        }

        private void OnFailedToStartMatchmakingGame()
        {
            _isMatchmaking = false;
            _isBattleStarting = false;
            UpdateVisibility();
        }

        private void OnStartTimeSet(long startTime)
        {
            _isBattleStarting = true;
            UpdateVisibility();
        }
    }
}
