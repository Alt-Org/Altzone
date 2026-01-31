using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using MenuUi.Scripts.Signals;

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

        private string _currentRegion;
        private Coroutine _creatingRoomCoroutineHolder = null;

        public static GameType SelectedGameType { get; private set; }

        private void Awake()
        {
            SignalBus.OnBattlePopupRequested += OpenWindow;
            SignalBus.OnCloseBattlePopupRequested += CloseWindow;
        }


        private void OnDestroy()
        {
            SignalBus.OnBattlePopupRequested -= OpenWindow;
            SignalBus.OnCloseBattlePopupRequested -= CloseWindow;
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

            // Checking if we are in room or matchmaking room depending on the game mode which would prevent changing the selected game type
            switch (gameType)
            {
                case GameType.Custom:
                    if (PhotonRealtimeClient.InRoom)
                    {
                        _roomSwitcher.SwitchRoom(GameType.Custom);
                        return;
                    }
                    break;
                case GameType.Clan2v2:
                case GameType.Random2v2:
                    if (PhotonRealtimeClient.InMatchmakingRoom) // If we are in matchmaking we don't want to do anything
                    {
                        return;
                    }
                    else if (PhotonRealtimeClient.InRoom) // If we are in a normal room
                    {
                        // Checking if the game type changed, if it didn't we don't want to do anything but if it did we leave the room
                        if (gameType == SelectedGameType)
                        {
                            return;
                        }
                        else
                        {
                            PhotonRealtimeClient.LeaveRoom();
                        }
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
    }
}
