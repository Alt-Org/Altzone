using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using MenuUi.Scripts.Signals;
using Altzone.Scripts.Battle.Photon;
using MenuUi.Scripts.Window;
using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.Signals
{
    public static partial class SignalBus
    {
        public delegate void BattlePopupRequestedHandler(GameType gameType);
        public static event BattlePopupRequestedHandler OnBattlePopupRequested;
        public static void OnBattlePopupRequestedSignal(GameType gameType)
        {
            OnBattlePopupRequested?.Invoke(gameType);
        }

        public delegate void CloseBattlePopupRequestedHandler();
        public static event CloseBattlePopupRequestedHandler OnCloseBattlePopupRequested;
        public static void OnCloseBattlePopupRequestedSignal()
        {
            OnCloseBattlePopupRequested?.Invoke();
        }

        public delegate void CustomRoomSettingsRequestedHandler();
        public static event CustomRoomSettingsRequestedHandler OnCustomRoomSettingsRequested;
        public static void OnCustomRoomSettingsRequestedSignal()
        {
            OnCustomRoomSettingsRequested?.Invoke();
        }
    }
}

namespace MenuUi.Scripts.Lobby.InLobby
{
    /// <summary>
    /// Handles opening and closing the battle popup and connects player to the photon lobby.
    /// </summary>
    public class InLobbyController : AltMonoBehaviour
    {
        [SerializeField] private TopInfoPanelController _topInfoPanel;
        [SerializeField] private GameObject _popupContents;
        [SerializeField] private BattlePopupPanelManager _roomSwitcher;
        [SerializeField] private LobbyRoomListingController _roomListingController;
        // Expose the runtime instance of the popup contents so other scene components can reference it at runtime.
        public static GameObject PopupContentsInstance { get; private set; }

        // Fired when `PopupContentsInstance` is assigned or cleared at runtime.
        public static event Action<GameObject> OnPopupContentsInstanceAssigned;
        private string _currentRegion;
        private Coroutine _creatingRoomCoroutineHolder = null;

        public static GameType SelectedGameType { get; private set; }
        public static GameType SelectedPremadeTargetGameType { get; private set; } = GameType.Random2v2;

        public static void SetPremadeTargetGameType(GameType gameType)
        {
            if (gameType == GameType.Random2v2 || gameType == GameType.Clan2v2)
            {
                SelectedPremadeTargetGameType = gameType;
            }
        }

        private void Awake()
        {
            SignalBus.OnBattlePopupRequested += OpenWindow;
            SignalBus.OnCloseBattlePopupRequested += CloseWindow;
            LobbyManager.OnInRoomInviteReceived += OnInRoomInviteReceived;
            LobbyManager.OnInRoomInviteJoinFailed += OnInRoomInviteJoinFailed;
            // Register runtime popup reference for other components to find (safe to set here because serialized field is available in Awake)
            PopupContentsInstance = _popupContents;
            OnPopupContentsInstanceAssigned?.Invoke(PopupContentsInstance);
        }


        private void OnDestroy()
        {
            SignalBus.OnBattlePopupRequested -= OpenWindow;
            SignalBus.OnCloseBattlePopupRequested -= CloseWindow;
            LobbyManager.OnInRoomInviteReceived -= OnInRoomInviteReceived;
            LobbyManager.OnInRoomInviteJoinFailed -= OnInRoomInviteJoinFailed;
            if (PopupContentsInstance == _popupContents)
            {
                PopupContentsInstance = null;
                OnPopupContentsInstanceAssigned?.Invoke(null);
            }
        }


        public void OnEnable()
        {
            //base.OnEnable();

            //var cloudRegion = PhotonNetwork.NetworkingClient?.CloudRegion;
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var photonRegion = string.IsNullOrEmpty(playerSettings.PhotonRegion) ? null : playerSettings.PhotonRegion;
            //Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState} CloudRegion={cloudRegion} PhotonRegion={photonRegion}");
            /*if (PhotonWrapper.IsConnectedToMasterServer && photonRegion != cloudRegion)
            {
                // We need to disconnect from current region because it is not the same as in player settings.
                PhotonLobby.Disconnect();
            }*/
            _topInfoPanel.Reset();
            UpdateTitle();
            _topInfoPanel.LobbyTextLiteral = string.Empty;
            //StartCoroutine(StartLobby(playerSettings.PlayerGuid, playerSettings.PhotonRegion));
        }

        public void OnDisable()
        {
            CloseWindow();
        }

        private void UpdateTitle()
        {
            // Save region for later use because getting it is not cheap (b ut not very expensive either). 
            _currentRegion = PhotonRealtimeClient.CloudRegion != null ? PhotonRealtimeClient.CloudRegion : "";
            _topInfoPanel.TitleText = $"{Application.productName} {PhotonRealtimeClient.GameVersion}";
        }

        private IEnumerator StartLobby(string playerGuid, string photonRegion)
        {
            var networkClientState = PhotonRealtimeClient.LobbyNetworkClientState;
            Debug.Log($"{networkClientState}");
            var delay = new WaitForSeconds(0.1f);
            while (!PhotonRealtimeClient.InLobby)
            {
                if (networkClientState != PhotonRealtimeClient.LobbyNetworkClientState)
                {
                    // Even with delay we must reduce NetworkClientState logging to only when it changes to avoid flooding (on slower connections).
                    networkClientState = PhotonRealtimeClient.LobbyNetworkClientState;
                    Debug.Log($"{networkClientState}");
                }
                if (PhotonRealtimeClient.InRoom)
                {
                    PhotonRealtimeClient.LeaveRoom();
                }
                else if (PhotonRealtimeClient.CanConnect)
                {
                    var store = Storefront.Get();
                    PlayerData playerData = null;
                    store.GetPlayerData(playerGuid, p => playerData = p);
                    yield return new WaitUntil(() => playerData != null);
                    PhotonRealtimeClient.Connect(playerData.Name, photonRegion);
                }
                else if (PhotonRealtimeClient.CanJoinLobby)
                {
                    PhotonRealtimeClient.JoinLobbyWithWrapper(null);
                }
                yield return delay;
            }
            UpdateTitle();
        }

        private void Update()
        {
            if (!PhotonRealtimeClient.InLobby && !PhotonRealtimeClient.InRoom)
            {
                _topInfoPanel.LobbyTextLiteral = "Wait";
                return;
            }
            UpdateTitle();
            var playerCount = PhotonRealtimeClient.CountOfPlayers;
            _topInfoPanel.LobbyText = new string[2] { _currentRegion, PhotonRealtimeClient.GetPing().ToString()};
            _topInfoPanel.PlayerCountText = playerCount.ToString();
            _topInfoPanel.MatchmakingCountText = PhotonRealtimeClient.CurrentRoomPlayerCount.ToString();
        }

        /*public void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"OnDisconnected {cause}");

            if (cause != DisconnectCause.DisconnectByClientLogic && cause != DisconnectCause.DisconnectByServerLogic)
            {
                OnEnable();
            }
        }*/


        private void OpenWindow(GameType gameType)
        {
            _popupContents.SetActive(true);
            // Ensure top info shows current values when popup opens
            RefreshTopInfo();

            // Checking if we are in room or matchmaking room depending on the game mode which would prevent changing the selected game type
            switch (gameType)
            {
                case GameType.Custom:
                    if (PhotonRealtimeClient.InRoom)
                    {
                        if (gameType == SelectedGameType)
                        {
                            _roomSwitcher.SwitchRoom(GameType.Custom);
                            return;
                        }
                        else
                        {
                            // Stop matchmaking coroutines before leaving to switch game type
                            LobbyManager.Instance.StopMatchmakingCoroutines();
                            PhotonRealtimeClient.LeaveRoom();
                        }
                    }
                    break;
                case GameType.Clan2v2:
                case GameType.Random2v2:
                    // Treat persistent queue rooms as matchmaking state so reopening the popup shows matchmaking panel
                    bool inQueueRoom = false;
                    try
                    {
                        var curr = PhotonRealtimeClient.LobbyCurrentRoom;
                        if (curr != null && curr.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey)) inQueueRoom = true;
                    }
                    catch { }

                    bool currentRoomGameTypeMatches = false;
                    try
                    {
                        var currRoom = PhotonRealtimeClient.LobbyCurrentRoom;
                        if (currRoom != null)
                        {
                            var gt = currRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                            currentRoomGameTypeMatches = gt == (int)gameType;
                        }
                    }
                    catch { }

                    if ((PhotonRealtimeClient.InMatchmakingRoom || inQueueRoom) && currentRoomGameTypeMatches)
                    {
                        _roomSwitcher.SwitchToMatchmakingPanel(PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient);
                        return;
                    }
                    else if (PhotonRealtimeClient.InRoom) // If we are in a room
                    {
                        // Checking if the game type changed, if it didn't we don't want to do anything but if it did we leave the room
                        if (currentRoomGameTypeMatches)
                        {
                            return;
                        }
                        else
                        {
                            // Stop matchmaking coroutines before leaving to switch game type
                            LobbyManager.Instance.StopMatchmakingCoroutines();
                            PhotonRealtimeClient.LeaveRoom();
                        }
                    }
                    break;
                case GameType.FriendLobby:
                    bool currentFriendRoomMatches = false;
                    try
                    {
                        var currRoom = PhotonRealtimeClient.LobbyCurrentRoom;
                        if (currRoom != null)
                        {
                            var gt = currRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                            currentFriendRoomMatches = gt == (int)gameType;
                        }
                    }
                    catch { }

                    if (PhotonRealtimeClient.InMatchmakingRoom && currentFriendRoomMatches)
                    {
                        _roomSwitcher.SwitchToMatchmakingPanel(PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient);
                        return;
                    }

                    if (PhotonRealtimeClient.InRoom)
                    {
                        if (currentFriendRoomMatches)
                        {
                            _roomSwitcher.SwitchRoom(GameType.FriendLobby);
                            return;
                        }

                        LobbyManager.Instance.StopMatchmakingCoroutines();
                        PhotonRealtimeClient.LeaveRoom();
                    }
                    break;
                default:
                    return;
            }

            SelectedGameType = gameType;

            // Starting creating room of a selected game type if the coroutine is not already running
            if (_creatingRoomCoroutineHolder != null) return;
            _roomSwitcher.ClosePanels();
            _creatingRoomCoroutineHolder = StartCoroutine(_roomListingController.StartCreatingRoom(gameType, () =>
            {
                _creatingRoomCoroutineHolder = null;
            }));
        }


        public void CloseWindow()
        {
            _roomSwitcher.ClosePanels();
            _popupContents.SetActive(false);
        }

        private void RefreshTopInfo()
        {
            try
            {
                UpdateTitle();
                if (_topInfoPanel != null)
                {
                    _topInfoPanel.LobbyText = new string[2] { _currentRegion, PhotonRealtimeClient.GetPing().ToString() };
                    _topInfoPanel.PlayerCountText = PhotonRealtimeClient.CountOfPlayers.ToString();
                    _topInfoPanel.MatchmakingCountText = PhotonRealtimeClient.CurrentRoomPlayerCount.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"RefreshTopInfo failed: {ex.Message}");
            }
        }

        private void CharacterButtonOnClick()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
        }

        private void RoomButtonOnClick()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
        }

        private void RaidButtonOnClick()
        {
            SceneManager.LoadScene("te-test-raid-demo");
        }

        private void QuickGameButtonOnClick()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
        }

        private void OnInRoomInviteReceived(LobbyManager.InRoomInviteInfo inviteInfo)
        {
            if (inviteInfo == null || string.IsNullOrEmpty(inviteInfo.RoomName)) return;

            string inviterName = ResolveOnlinePlayerName(inviteInfo.LeaderUserId);
            string targetMode = inviteInfo.TargetGameType == GameType.Clan2v2 ? "Clan 2v2" : "Random 2v2";
            string message = $"{inviterName} kutsui sinut Friend Lobby -huoneeseen. Haettava pelimuoto: {targetMode}. Liitytaanko huoneeseen?";

            bool popupShown = InviteDecisionPopupHandler.RequestInviteDecisionPrompt(
                message,
                "Liity",
                "Hylkää",
                accepted =>
                {
                    if (LobbyManager.Instance == null) return;
                    if (accepted)
                    {
                        OpenBattlePopupForInviteAccept();
                        LobbyManager.Instance.AcceptInRoomInvite(inviteInfo.RoomName);
                    }
                    else LobbyManager.Instance.DeclineInRoomInvite(inviteInfo.RoomName);
                });

            if (!popupShown)
            {
                Debug.LogWarning("OnInRoomInviteReceived: decision popup unavailable, auto-joining invite.");
                OpenBattlePopupForInviteAccept();
                LobbyManager.Instance?.AcceptInRoomInvite(inviteInfo.RoomName);
                PopupSignalBus.OnChangePopupInfoSignal("Friend Lobby -kutsu saatu, liityttiin automaattisesti.");
            }
        }

        private void OpenBattlePopupForInviteAccept()
        {
            SelectedGameType = GameType.FriendLobby;

            if (_popupContents != null && !_popupContents.activeSelf)
            {
                _popupContents.SetActive(true);
            }

            RefreshTopInfo();
            _roomSwitcher?.SwitchRoom(GameType.FriendLobby);
        }

        private string ResolveOnlinePlayerName(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return "Pelaaja";

            try
            {
                var onlinePlayers = ServerManager.Instance?.OnlinePlayers;
                if (onlinePlayers != null)
                {
                    foreach (ServerOnlinePlayer player in onlinePlayers)
                    {
                        if (player == null || player._id != userId) continue;
                        if (!string.IsNullOrWhiteSpace(player.name)) return player.name;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"ResolveOnlinePlayerName failed: {ex.Message}");
            }

            return userId;
        }

        private void OnInRoomInviteJoinFailed(string roomName, short returnCode, string message)
        {
            bool isRoomFull = returnCode == 32765
                || (!string.IsNullOrEmpty(message) && message.ToLowerInvariant().Contains("game full"));

            string popupMessage = isRoomFull
                ? "Friend Lobby -kutsuun liittyminen epaonnistui: huone on taynna tai kutsu ei ole enaa voimassa."
                : "Friend Lobby -kutsuun liittyminen epaonnistui. Yrita uudelleen, jos kutsu on yha voimassa.";

            PopupSignalBus.OnChangePopupInfoSignal(popupMessage);
        }
    }
}
