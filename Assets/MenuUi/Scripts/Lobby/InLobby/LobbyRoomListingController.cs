using System;
using System.Collections;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Prg.Scripts.Common.PubSub;

using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Common.Photon;
using Altzone.Scripts.Lobby;
using Photon.Client;
using ReasonType = Altzone.Scripts.Lobby.LobbyManager.GetKickedEvent.ReasonType;

using MenuUi.Scripts.Lobby.CreateRoom;
using PopupSignalBus = MenuUI.Scripts.SignalBus;
using MenuUi.Scripts.Signals;

namespace MenuUi.Scripts.Lobby.InLobby
{
    /// <summary>
    /// Handles calling the photon methods for creating a new room or joining a room, and forwarding the photon room list to RoomSearchPanelController.
    /// </summary>
    public class LobbyRoomListingController : AltMonoBehaviour
    {
        private const string DefaultRoomNameCustom = "Custom ";

        [SerializeField] private RoomSearchPanelController _searchPanel;
        [SerializeField] private BattlePopupPanelManager _roomSwitcher;
        [SerializeField] private CreateRoomCustom _createRoomCustom;
        [SerializeField] private Button _createRoomFromMainMenuButton;
        [SerializeField] private PasswordPopup _passwordPopup;
        [SerializeField] private GameObject _creatingRoomText;

        private PhotonRoomList _photonRoomList;
        private JoinIntent _pendingJoinIntent = JoinIntent.None;
        private GameType _pendingQueueGameType = GameType.Random2v2;
        private Coroutine _queueRejoinHolder;

        private enum JoinIntent
        {
            None,
            QueueJoin,
            CustomCreate,
            ManualRoomJoin
        }

        private void Awake()
        {
            _photonRoomList = gameObject.GetOrAddComponent<PhotonRoomList>();
            _createRoomCustom.CreateRoomButton.onClick.AddListener(CreateCustomRoom);
            _createRoomFromMainMenuButton.onClick.AddListener(CreateCustomRoom);
            LobbyManager.OnClanMemberDisconnected += HandleClanMemberDisconnected;
            LobbyManager.OnKickedOutOfTheRoom += HandleKickedOutOfRoom;
        }

        public void OnEnable()
        {
            PhotonRealtimeClient.AddCallbackTarget(this);
            Debug.Log($"OnEnable {PhotonRealtimeClient.LobbyNetworkClientState}");
            if (PhotonRealtimeClient.InLobby)
            {
                UpdateStatus();
            }
            // If already in matchmaking or queue room, ensure creating-room text is hidden
            try
            {
                bool isMatchmakingOrQueue = PhotonRealtimeClient.InMatchmakingRoom;
                var curr = PhotonRealtimeClient.LobbyCurrentRoom;
                if (curr != null && curr.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey, false)) isMatchmakingOrQueue = true;
                if (isMatchmakingOrQueue && _creatingRoomText != null && _creatingRoomText.activeSelf) _creatingRoomText.SetActive(false);
            }
            catch { }
            _photonRoomList.OnRoomsUpdated += UpdateStatus;
            LobbyManager.LobbyOnJoinedRoom += OnJoinedRoom;
            LobbyManager.OnMatchmakingRoomEntered += HandleMatchmakingRoomEntered;
            LobbyManager.LobbyOnJoinRoomFailed += OnJoinedRoomFailed;
            LobbyWindowNavigationHandler.OnLobbyWindowChangeRequest += SwitchToRoom;
        }

        public void OnDisable()
        {
            PhotonRealtimeClient.RemoveCallbackTarget(this);
            _photonRoomList.OnRoomsUpdated -= UpdateStatus;
            LobbyManager.LobbyOnJoinedRoom -= OnJoinedRoom;
            LobbyManager.OnMatchmakingRoomEntered -= HandleMatchmakingRoomEntered;
            LobbyManager.LobbyOnJoinRoomFailed -= OnJoinedRoomFailed;
            LobbyWindowNavigationHandler.OnLobbyWindowChangeRequest -= SwitchToRoom;
            if (_queueRejoinHolder != null)
            {
                StopCoroutine(_queueRejoinHolder);
                _queueRejoinHolder = null;
            }
            _pendingJoinIntent = JoinIntent.None;
        }

        private void OnDestroy()
        {
            LobbyManager.OnClanMemberDisconnected -= HandleClanMemberDisconnected;
            LobbyManager.OnKickedOutOfTheRoom -= HandleKickedOutOfRoom;
            _createRoomCustom.CreateRoomButton.onClick.RemoveAllListeners();
            _createRoomFromMainMenuButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Coroutine to create a room after client is connected to lobby.
        /// </summary>
        /// <param name="gameType">Game type of which room to create.</param>
        /// <param name="callback">Callback which is called after room is created.</param>
        /// <returns></returns>
        public IEnumerator StartCreatingRoom(GameType gameType, Action callback)
        {
            // Do not show the creating-room text if the client is in a matchmaking or queue room
            bool isMatchmakingOrQueue = PhotonRealtimeClient.InMatchmakingRoom;
            try
            {
                var curr = PhotonRealtimeClient.LobbyCurrentRoom;
                if (curr != null && curr.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey, false)) isMatchmakingOrQueue = true;
            }
            catch { }

            if (!isMatchmakingOrQueue)
            {
                _creatingRoomText.SetActive(true);
       
            }
            bool roomCreated = false;
            do
            {
                yield return null;
                if (PhotonRealtimeClient.InLobby)
                {
                    switch (gameType)
                    {
                        case GameType.FriendLobby:
                            CreateInRoomPremadeRoom();
                            break;
                        case GameType.Clan2v2:
                            CreateClan2v2Room();
                            break;
                        case GameType.Random2v2:
                            CreateRandom2v2Room();
                            break;
                        case GameType.Custom:
                            if (!_createRoomCustom.IsCustomRoomOptionsReady) _createRoomCustom.InitializeCustomRoomOptions();
                            CreateCustomRoom();
                            break;
                    }
                    roomCreated = true;
                }
            } while (!roomCreated);

            callback();
        }

        private void CreateCustomRoom()
        {
            _pendingJoinIntent = JoinIntent.CustomCreate;
            // int randomNumber = UnityEngine.Random.Range(0, 100) + DateTime.Now.Millisecond;
            string roomName = string.IsNullOrWhiteSpace(_createRoomCustom.RoomName) ? $"{DefaultRoomNameCustom}" : $"{_createRoomCustom.RoomName}";

            if (_createRoomCustom.IsPrivate && _createRoomCustom.RoomPassword != null && _createRoomCustom.RoomPassword != "")
            {
                PhotonRealtimeClient.CreateCustomLobbyRoom(roomName, _createRoomCustom.SelectedMapId, _createRoomCustom.SelectedEmotion, _createRoomCustom.RoomPassword);
            }
            else
            {
                PhotonRealtimeClient.JoinRandomOrCreateCustomRoom(roomName, _createRoomCustom.SelectedMapId, _createRoomCustom.SelectedEmotion);
            }
        }

        private void CreateClan2v2Room()  // soulhome value for matchmaking
        {
            StartCoroutine(GetClanData( clanData =>
            {
                if (clanData != null)
                {
                    // Join the persistent queue room instead of creating a matchmaking room immediately
                    _pendingJoinIntent = JoinIntent.QueueJoin;
                    _pendingQueueGameType = GameType.Clan2v2;
                    PhotonRealtimeClient.JoinOrCreateQueueRoom(GameType.Clan2v2);
                }
            }));
        }

        private void CreateRandom2v2Room()  // soulhome value for matchmaking
        {
            StartCoroutine(GetClanData(clanData =>
            {
                if (clanData != null)
                {
                    // Join the persistent queue room instead of creating a matchmaking room immediately
                    _pendingJoinIntent = JoinIntent.QueueJoin;
                    _pendingQueueGameType = GameType.Random2v2;
                    PhotonRealtimeClient.JoinOrCreateQueueRoom(GameType.Random2v2);
                }
            }));
        }

        private void CreateInRoomPremadeRoom()
        {
            PhotonRealtimeClient.CreateInRoomPremadeLobbyRoom();
        }

        private void JoinRoom(string roomName)
        {
            Debug.Log($"{roomName}");
            var rooms = _photonRoomList.CurrentRooms.ToList();
            foreach (var roomInfo in rooms)
            {
                if (roomInfo.Name.Equals(roomName, StringComparison.Ordinal) && !roomInfo.RemovedFromList && roomInfo.IsOpen)
                {
                    // Asking for password if there is one
                    if (roomInfo.CustomProperties.ContainsKey(PhotonBattleRoom.PasswordKey))
                    {
                        string password = (string)roomInfo.CustomProperties[PhotonBattleRoom.PasswordKey];
                        StartCoroutine(_passwordPopup.AskForPassword(passwordInput =>
                        {
                            if (password.Trim() == passwordInput.Trim())
                            {
                                _pendingJoinIntent = JoinIntent.ManualRoomJoin;
                                PhotonRealtimeClient.JoinRoom(roomInfo.Name);
                            }
                            else
                            {
                                PopupSignalBus.OnChangePopupInfoSignal("Salasana on väärin.");
                            }
                            _passwordPopup.ClosePopup();
                        }));
                        return;
                    }

                    _pendingJoinIntent = JoinIntent.ManualRoomJoin;
                    PhotonRealtimeClient.JoinRoom(roomInfo.Name);
                    break;
                }
            }
        }

        public void OnJoinedRoom()
        {
            _pendingJoinIntent = JoinIntent.None;
            if (_creatingRoomText.activeSelf) _creatingRoomText.SetActive(false);
            var room = PhotonRealtimeClient.LobbyCurrentRoom; // hakee pelaajan tiedot // 
            var player = PhotonRealtimeClient.LocalLobbyPlayer;
            //PhotonRealtimeClient.NickName = room.GetUniquePlayerNameForRoom(player, PhotonRealtimeClient.NickName, "");
            Debug.Log($"'{room.Name}' player name '{PhotonRealtimeClient.NickName}'");
            this.Publish(new LobbyManager.StartRoomEvent());
        }

        private void HandleMatchmakingRoomEntered(bool isLeader)
        {
            try
            {
                if (_creatingRoomText != null && _creatingRoomText.activeSelf) _creatingRoomText.SetActive(false);
            }
            catch { }
        }

        public void OnJoinedRoomFailed(short returnCode, string message)
        {
            bool creatingTextActive = _creatingRoomText != null && _creatingRoomText.activeSelf;

            // Only attempt to create a custom room automatically when the user was creating a custom room.
            if (_pendingJoinIntent == JoinIntent.CustomCreate || (creatingTextActive && InLobbyController.SelectedGameType == GameType.Custom))
            {
                CreateCustomRoom();
                return;
            }

            if (ShouldRejoinQueueAfterJoinFailed(returnCode, message, out GameType queueGameType))
            {
                if (creatingTextActive) _creatingRoomText.SetActive(false);
                _pendingJoinIntent = JoinIntent.None;
                StartQueueRejoin(queueGameType);
                return;
            }

            if (creatingTextActive) _creatingRoomText.SetActive(false);
            _pendingJoinIntent = JoinIntent.None;
            PopupSignalBus.OnChangePopupInfoSignal("Liityminen epäonnistui: huone on täysi tai ei käytettävissä.");
        }

        private bool ShouldRejoinQueueAfterJoinFailed(short returnCode, string message, out GameType queueGameType)
        {
            queueGameType = _pendingQueueGameType;

            if (_pendingJoinIntent == JoinIntent.ManualRoomJoin)
            {
                return false;
            }

            if (_pendingJoinIntent == JoinIntent.QueueJoin)
            {
                queueGameType = _pendingQueueGameType;
            }
            else if (InLobbyController.SelectedGameType == GameType.Random2v2 || InLobbyController.SelectedGameType == GameType.Clan2v2)
            {
                queueGameType = InLobbyController.SelectedGameType;
            }
            else
            {
                return false;
            }

            bool retryableCode = returnCode == Altzone.Scripts.Lobby.Wrappers.LobbyErrorCode.GameFull
                                 || returnCode == Altzone.Scripts.Lobby.Wrappers.LobbyErrorCode.GameClosed
                                 || returnCode == Altzone.Scripts.Lobby.Wrappers.LobbyErrorCode.GameDoesNotExist;
            if (retryableCode)
            {
                return true;
            }

            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            string msg = message.ToLowerInvariant();
            return msg.Contains("game full") || msg.Contains("game closed") || msg.Contains("does not exist");
        }

        private void StartQueueRejoin(GameType gameType)
        {
            if (_queueRejoinHolder != null)
            {
                return;
            }

            _queueRejoinHolder = StartCoroutine(RejoinQueueWhenLobbyReady(gameType));
        }

        private IEnumerator RejoinQueueWhenLobbyReady(GameType gameType)
        {
            try
            {
                float waitStart = Time.time;
                while (PhotonRealtimeClient.InRoom && Time.time - waitStart < 6f)
                {
                    yield return null;
                }

                while (!PhotonRealtimeClient.InLobby && Time.time - waitStart < 6f)
                {
                    yield return null;
                }

                if (!PhotonRealtimeClient.InLobby)
                {
                    Debug.LogWarning($"RejoinQueueWhenLobbyReady: not in lobby, skip queue rejoin for {gameType}");
                    PopupSignalBus.OnChangePopupInfoSignal("Liityminen epäonnistui: huone on täysi tai ei käytettävissä.");
                    yield break;
                }

                bool inMatchmakingOrQueue = PhotonRealtimeClient.InMatchmakingRoom;
                try
                {
                    var room = PhotonRealtimeClient.LobbyCurrentRoom;
                    if (room != null && room.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey, false)) inMatchmakingOrQueue = true;
                }
                catch { }

                if (inMatchmakingOrQueue)
                {
                    yield break;
                }

                bool joined;
                try
                {
                    joined = PhotonRealtimeClient.JoinOrCreateQueueRoom(gameType);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"RejoinQueueWhenLobbyReady: JoinOrCreateQueueRoom threw: {ex.Message}");
                    PopupSignalBus.OnChangePopupInfoSignal("Liityminen epäonnistui: huone on täysi tai ei käytettävissä.");
                    yield break;
                }

                if (!joined)
                {
                    Debug.LogWarning($"RejoinQueueWhenLobbyReady: JoinOrCreateQueueRoom returned false for {gameType}");
                    PopupSignalBus.OnChangePopupInfoSignal("Liityminen epäonnistui: huone on täysi tai ei käytettävissä.");
                }
            }
            finally
            {
                _queueRejoinHolder = null;
            }
        }

        public void SwitchToRoom()
        {
            _roomSwitcher.SwitchRoom(InLobbyController.SelectedGameType);
        }

        private void UpdateStatus()
        {
            LobbyManager.Instance.CurrentRooms = _photonRoomList.CurrentRooms;
            var rooms = _photonRoomList.CurrentRooms.ToList();
            rooms.Sort((a, b) =>
            {
                // First open rooms by name, then closed (aka playing) rooms by name
                var strA = $"{(a.IsOpen ? 0 : 1)}{a.Name}";
                var strB = $"{(b.IsOpen ? 0 : 1)}{b.Name}";
                return string.Compare(strA, strB, StringComparison.Ordinal);
            });
            _searchPanel.RoomsData = rooms;
            _searchPanel.SetOnJoinRoom(JoinRoom);
        }

        private void HandleClanMemberDisconnected()
        {
            PopupSignalBus.OnChangePopupInfoSignal("Pelin etsiminen lopetetaan. Klaanin jäsen sulki pelin.");
        }

        private void HandleKickedOutOfRoom(ReasonType reason)
        {
            switch (reason)
            {
                case ReasonType.FullRoom:
                    PopupSignalBus.OnChangePopupInfoSignal("Virhe pelin etsimisessä, huone on täysi.");
                    //CreateCustomRoom();
                    break;
                case ReasonType.RoomLeader:
                    PopupSignalBus.OnChangePopupInfoSignal("Huoneen johtaja poisti sinut huoneesta.");
                    //CreateCustomRoom();
                    break;
            }
            SignalBus.OnCloseBattlePopupRequestedSignal();
        }
    }
}
