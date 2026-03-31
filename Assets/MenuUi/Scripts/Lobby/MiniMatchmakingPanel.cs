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
        private int _popupSearchAttempts;
        private const int MaxPopupSearchAttempts = 120;

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

            // Fallback: try to find the battle popup at runtime if it's in a separate prefab
            TryFindBattlePopup();

            // Try to auto-assign matchmaking count text if inspector field is empty
            TryFindMatchmakingCountText();

            SetVisible(false);

            // Initialize state from current Photon/room state in case matchmaking started before this component was enabled
            TryInitFromCurrentState();
            _popupSearchAttempts = 0; // Initialize popup search attempts
            TryFindBattlePopup(); // Attempt to find the battle popup
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
                    SetMatchmakingTextForCurrentRoom();
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
            // Retry locating the battle popup (prefer scene instance) for a short time if not found yet
            if ((_battlePopup == null || !_battlePopup.scene.IsValid()) && _popupSearchAttempts < MaxPopupSearchAttempts)
            {
                _popupSearchAttempts++;
                if (TryFindBattlePopupSceneOnly())
                {
                    UpdateCancelButtonFromState();
                    TryFindMatchmakingCountText();
                    UpdateVisibility();
                }
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

        private void TryFindBattlePopup()
        {
            if (_battlePopup != null) return;
            // Try scene-only search first
            if (TryFindBattlePopupSceneOnly()) return;

            // If immediate scene search failed, fallback to asset/inactive search (less desirable)
            var mgrs = Resources.FindObjectsOfTypeAll<BattlePopupPanelManager>();
            foreach (var m in mgrs)
            {
                if (m == null) continue;
                var go = m.gameObject;
                if (go != null)
                {
                    _battlePopup = go;
                    Debug.Log($"MiniMatchmakingPanel: Found battle popup (asset/inactive) via BattlePopupPanelManager: {_battlePopup.name}");
                    return;
                }
            }

            var gos = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var g in gos)
            {
                if (g == null) continue;
                string n = g.name.ToLowerInvariant();
                if (n.Contains("popup") || n.Contains("battle") || n.Contains("popupcontents"))
                {
                    _battlePopup = g;
                    Debug.Log($"MiniMatchmakingPanel: Found battle popup (asset/inactive) via name match: {_battlePopup.name}");
                    return;
                }
            }

            Debug.Log("MiniMatchmakingPanel: Battle popup not found by any fallback.");
        }

        private bool TryFindBattlePopupSceneOnly()
        {
            // Prefer explicit runtime registration from InLobbyController
            var popupInst = InLobbyController.PopupContentsInstance;
            if (popupInst != null && popupInst.scene.IsValid() && popupInst.scene.isLoaded)
            {
                _battlePopup = popupInst;
                Debug.Log($"MiniMatchmakingPanel: Found battle popup via InLobbyController: {_battlePopup.name}");
                return true;
            }

            // Prefer a BattlePopupPanelManager scene instance
            var mgrs = Resources.FindObjectsOfTypeAll<BattlePopupPanelManager>();
            foreach (var m in mgrs)
            {
                if (m == null) continue;
                var go = m.gameObject;
                if (go == null) continue;
                var scene = go.scene;
                if (scene.IsValid() && scene.isLoaded)
                {
                    _battlePopup = go;
                    Debug.Log($"MiniMatchmakingPanel: Found battle popup (scene instance) via BattlePopupPanelManager: {_battlePopup.name}");
                    return true;
                }
            }

            // Prefer a MatchmakingPanel scene instance and climb to a reasonable popup root
            var mps = Resources.FindObjectsOfTypeAll<MatchmakingPanel>();
            foreach (var mp in mps)
            {
                if (mp == null) continue;
                var go = mp.gameObject;
                if (go == null) continue;
                var scene = go.scene;
                if (!(scene.IsValid() && scene.isLoaded)) continue;
                var candidate = FindPopupRootFrom(go);
                _battlePopup = candidate ?? go;
                Debug.Log($"MiniMatchmakingPanel: Found battle popup (scene instance) via MatchmakingPanel: {_battlePopup.name}");
                return true;
            }

            // As a last resort, search scene GameObjects that contain MatchmakingPanel or BattlePopupPanelManager in their descendants
            var gos = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var g in gos)
            {
                if (g == null) continue;
                var scene = g.scene;
                if (!(scene.IsValid() && scene.isLoaded)) continue;
                if (g.GetComponentsInChildren<MatchmakingPanel>(true).Length > 0 || g.GetComponentsInChildren<BattlePopupPanelManager>(true).Length > 0)
                {
                    _battlePopup = g;
                    Debug.Log($"MiniMatchmakingPanel: Found battle popup (scene instance) via descendant component: {_battlePopup.name}");
                    return true;
                }
            }

            return false;
        }

        private GameObject FindPopupRootFrom(GameObject g)
        {
            if (g == null) return null;
            var t = g.transform;
            GameObject last = g;
            while (t.parent != null)
            {
                var p = t.parent.gameObject;
                if (p == null) break;
                var pn = p.name.ToLowerInvariant();
                // Prefer parent that explicitly looks like a popup/root
                if (pn.Contains("popup") || pn.Contains("battle") || pn.Contains("popupcontents") || pn.Contains("window") || pn.Contains("uicanvas"))
                {
                    last = p;
                }
                t = t.parent;
            }
            return last;
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
                SetMatchmakingTextForCurrentRoom();
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
            try
            {
                var lobbyRoom = PhotonRealtimeClient.LobbyCurrentRoom;
                if (lobbyRoom != null && lobbyRoom.GetCustomProperty<bool>(Altzone.Scripts.Battle.Photon.PhotonBattleRoom.IsQueueKey, false))
                {
                    Debug.Log("MiniMatchmakingPanel: leaving queue room immediately due to user cancel.");
                    PhotonRealtimeClient.LeaveRoom();
                }
            }
            catch { }

            this.Publish(new LobbyManager.StopMatchmakingEvent(InLobbyController.SelectedGameType, true));
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
            // If we are still in a matchmaking room, resume matchmaking UI state.
            if (PhotonRealtimeClient.InMatchmakingRoom)
            {
                _isMatchmaking = true;
                _isBattleStarting = false;
                _matchmakingStartTime = Time.time;
                SetMatchmakingTextForCurrentRoom();
                bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
                SetCancelButton(isLeader);
            }
            else
            {
                // We left matchmaking (CancelGameStart forced return to main menu) - hide panel.
                _isMatchmaking = false;
                _isBattleStarting = false;
            }
            UpdateVisibility();
        }

        private void SetMatchmakingTextForCurrentRoom()
        {
            if (_matchmakingText == null) return;

            try
            {
                // Only show matchmaking text while actually in a matchmaking room or a persistent queue room.
                bool inMatchmakingOrQueue = false;
                try
                {
                    inMatchmakingOrQueue = PhotonRealtimeClient.InMatchmakingRoom;
                }
                catch { }

                var room = PhotonRealtimeClient.LobbyCurrentRoom;
                bool isQueue = false;
                try
                {
                    if (room != null)
                    {
                        isQueue = room.GetCustomProperty<bool>(Altzone.Scripts.Battle.Photon.PhotonBattleRoom.IsQueueKey);
                        if (isQueue) inMatchmakingOrQueue = true;
                    }
                }
                catch { }

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
