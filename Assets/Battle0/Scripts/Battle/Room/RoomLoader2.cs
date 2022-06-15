using System;
using System.Collections;
using System.Linq;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Battle0.Scripts.Battle.Room
{
    /// <summary>
    /// Game room loader to establish a well known state for the room before actual gameplay starts.
    /// </summary>
    /// <remarks>
    /// Can create a test room environment for more than one player.<br />
    /// Note that <c>RoomLoaderProduction</c> contains production code, rest is just for testing.
    /// </remarks>
    internal class RoomLoader2 : MonoBehaviourPunCallbacks
    {
        [Serializable]
        internal class DebugSettings
        {
            public const string TestRoomName = "TestRoom4";

            private const string Tooltip1 = "If 'Is Offline Mode' only one player can play";
            private const string Tooltip2 = "Player start position for single player test";
            private const string Tooltip3 = "If 'Player Pos' > 1 then this is automatic";
            private const string Tooltip4 = "Maybe test against bots or wall";

            [Tooltip(Tooltip1)] public bool _isOfflineMode;
            [Tooltip(Tooltip2), Range(1, 4)] public int _playerPos = 1;
            [Range(1, 4), Tooltip(Tooltip3)] public int _minPlayersToStart = 1;
            [Tooltip(Tooltip4)] public bool _isFillTeamBlueFirst;
            public int _currentPlayersInRoom;
        }

        [Serializable]
        internal class DebugUISettings
        {
            public Canvas _canvas;
            public TMP_Text _roomInfoText;
            public Button _playNowButton;
        }

        [Header("Settings"), SerializeField] private GameObject[] _objectsToActivate;

        [SerializeField, Header("Debug Settings")] private DebugSettings _debug;
        [SerializeField] private DebugUISettings _debugUISettings;

        private RoomLoaderDebugUi _debugUi;

        private void Awake()
        {
            Debug.Log($"Awake {PhotonNetwork.NetworkClientState}");
            if (_objectsToActivate.Any(x => x.activeSelf))
            {
                Debug.LogError("objectsToActivate has active items, disable them and retry");
                enabled = false;
                return;
            }
            _debugUi = new RoomLoaderDebugUi(_debugUISettings);
            if (PhotonNetwork.InRoom)
            {
                // Normal logic is that we are in a room and just do what we must do and continue.
                RoomLoaderProduction.ContinueToNextStage(this, _objectsToActivate);
                enabled = false;
                return;
            }
            if (_debug._minPlayersToStart > 1)
            {
                _debugUi.Show();
                _debugUi.SetWaitText(_debug._minPlayersToStart);
                _debugUi.PlayNowButtonOnClick = CloseRoomToStartPlay;
            }
            Debug.Log($"Awake and create test room {PhotonNetwork.NetworkClientState}");
        }

        public override void OnEnable()
        {
            // Create a test room - in offline (faster to create) or online (real thing) mode.
            base.OnEnable();
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.JoinedLobby)
            {
                // JoinOrCreateRoom -> OnJoinedRoom -> WaitForPlayersToArrive -> ContinueToNextStage
                OnJoinedLobby();
                return;
            }
            var isStateValid = state == ClientState.PeerCreated || state == ClientState.Disconnected || state == ClientState.ConnectedToMasterServer;
            if (!isStateValid)
            {
                throw new UnityException($"OnEnable: invalid connection state {PhotonNetwork.NetworkClientState}");
            }
            var playerName = PhotonBattle.GetLocalPlayerName();
            Debug.Log($"connect {PhotonNetwork.NetworkClientState} isOfflineMode {_debug._isOfflineMode} player {playerName}");
            PhotonNetwork.OfflineMode = _debug._isOfflineMode;
            PhotonNetwork.NickName = playerName;
            if (_debug._isOfflineMode)
            {
                // JoinRandomRoom -> OnJoinedRoom -> WaitForPlayersToArrive -> ContinueToNextStage
                _debug._minPlayersToStart = 1;
                PhotonNetwork.JoinRandomRoom();
            }
            else if (state == ClientState.ConnectedToMasterServer)
            {
                // JoinLobby -> JoinOrCreateRoom -> OnJoinedRoom -> WaitForPlayersToArrive -> ContinueToNextStage
                PhotonLobby.JoinLobby();
            }
            else
            {
                // Connect -> JoinLobby -> JoinOrCreateRoom -> OnJoinedRoom -> WaitForPlayersToArrive -> ContinueToNextStage
                PhotonLobby.Connect(playerName);
            }
        }

        private void CloseRoomToStartPlay()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState} {PhotonNetwork.CurrentRoom.GetDebugLabel()}");
            if (PhotonNetwork.InRoom
                && PhotonNetwork.CurrentRoom.IsOpen
                && PhotonNetwork.IsMasterClient)
            {
                PhotonLobby.CloseRoom(true);
            }
        }

        public override void OnConnectedToMaster()
        {
            if (!_debug._isOfflineMode)
            {
                Debug.Log($"joinLobby {PhotonNetwork.NetworkClientState}");
                PhotonLobby.JoinLobby();
            }
        }

        public override void OnJoinedLobby()
        {
            Debug.Log($"createRoom {PhotonNetwork.NetworkClientState}");
            PhotonLobby.JoinOrCreateRoom(DebugSettings.TestRoomName);
        }

        public override void OnJoinedRoom()
        {
            int GetPlayerPosFromPlayerCount(int playerCount)
            {
                if (_debug._minPlayersToStart == 1)
                {
                    return _debug._playerPos;
                }
                if (_debug._isFillTeamBlueFirst)
                {
                    // Shield visibility testing needs two players on same team.
                    return PhotonBattle.PlayerPosition1 + playerCount - 1;
                }
                // Distribute players evenly on both teams.
                switch (playerCount)
                {
                    case 1:
                        return PhotonBattle.PlayerPosition1;
                    case 2:
                        return PhotonBattle.PlayerPosition3;
                    case 3:
                        return PhotonBattle.PlayerPosition2;
                    case 4:
                        return PhotonBattle.PlayerPosition4;
                    default:
                        // Should not happen as Master Client closes room ASAP when it is full.
                        return -1;
                }
            }

            var room = PhotonNetwork.CurrentRoom;
            _debug._playerPos = GetPlayerPosFromPlayerCount(room.PlayerCount);
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            var model = PhotonBattle.GetCharacterModelForRoom(player);
            var playerMainSkill = (int)model.MainDefence;
            PhotonBattle.SetDebugPlayerProps(player, _debug._playerPos, playerMainSkill);
            Debug.Log($"OnJoinedRoom {player.GetDebugLabel()}");
            StartCoroutine(WaitForPlayersToArrive());
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log($"OnRoomPropertiesUpdate {room.GetDebugLabel()}");
            if (!room.IsOpen)
            {
                // Somebody has closed the room, we can continue as if Start button was pressed.
                ContinueToNextStage();
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"OnJoinRoomFailed {returnCode} {message}");
            _debugUi.SetErrorMessage($"Join failed: {message}");
        }

        private IEnumerator WaitForPlayersToArrive()
        {
            bool CanContinueToNextStage()
            {
                if (!PhotonNetwork.InRoom)
                {
                    return true;
                }
                var room = PhotonNetwork.CurrentRoom;
                if (!room.IsOpen)
                {
                    return true;
                }
                _debug._currentPlayersInRoom = PhotonBattle.CountRealPlayers();
                _debugUi.SetWaitText(_debug._minPlayersToStart - _debug._currentPlayersInRoom);
                return _debug._currentPlayersInRoom >= _debug._minPlayersToStart;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                _debugUi.EnableButtons();
            }
            else
            {
                _debugUi.HideButtons();
            }
            StartCoroutine(_debugUi.Blink(0.6f, 0.3f));
            yield return new WaitUntil(CanContinueToNextStage);
            ContinueToNextStage();
        }

        private void ContinueToNextStage()
        {
            StopAllCoroutines();
            if (!PhotonNetwork.InRoom)
            {
                Debug.Log($"{PhotonNetwork.NetworkClientState}");
                _debugUi.SetErrorMessage("Not in room");
                return;
            }
            Debug.Log($"{PhotonNetwork.NetworkClientState} {PhotonNetwork.CurrentRoom.GetDebugLabel()}");
            var player = PhotonNetwork.LocalPlayer;
            var playerPos = PhotonBattle.GetPlayerPos(player);
            if (!PhotonBattle.IsValidPlayerPos(playerPos))
            {
                _debugUi.SetErrorMessage("Invalid player");
                PhotonNetwork.LeaveRoom();
                return;
            }
            _debugUi.Hide();
            RoomLoaderProduction.ContinueToNextStage(this, _objectsToActivate);
        }

        private static class RoomLoaderProduction
        {
            public static void ContinueToNextStage(MonoBehaviour host, GameObject[] objectsToActivate)
            {
                Debug.Log($"{PhotonNetwork.NetworkClientState} {PhotonNetwork.CurrentRoom.GetDebugLabel()} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.IsOpen)
                {
                    PhotonLobby.CloseRoom(true);
                }
                // Disable: PhotonNetwork.CloseConnection needs to to work across all clients - to kick off invalid players!
                PhotonNetwork.EnableCloseConnection = false;
                // Enable game objects when this room stage is ready to play
                host.StartCoroutine(ActivateObjects(objectsToActivate));
            }

            private static IEnumerator ActivateObjects(GameObject[] objectsToActivate)
            {
                // Enable game objects one per frame in array sequence
                for (var i = 0; i < objectsToActivate.LongLength; i++)
                {
                    yield return null;
                    objectsToActivate[i].SetActive(true);
                }
            }
        }

        /// <summary>
        /// Helper class for debug UI without null checking all the time for our caller.
        /// </summary>
        private class RoomLoaderDebugUi
        {
            private readonly bool _isValid;
            private readonly Canvas _canvas;
            private readonly TMP_Text _roomInfoText;
            private readonly Button _playNowButton;

            public RoomLoaderDebugUi(DebugUISettings debugUISettings)
            {
                _isValid = debugUISettings._canvas != null;
                _canvas = debugUISettings._canvas;
                _roomInfoText = debugUISettings._roomInfoText;
                _playNowButton = debugUISettings._playNowButton;
                Hide();
            }

            public void Show()
            {
                if (!_isValid)
                {
                    return;
                }
                _canvas.gameObject.SetActive(true);
                _playNowButton.interactable = false;
            }

            public void Hide()
            {
                if (!_isValid)
                {
                    return;
                }
                _canvas.gameObject.SetActive(false);
            }

            public void EnableButtons()
            {
                if (!_isValid)
                {
                    return;
                }
                _playNowButton.interactable = true;
            }

            public void HideButtons()
            {
                if (!_isValid)
                {
                    return;
                }
                _playNowButton.gameObject.SetActive(false);
            }

            public void SetErrorMessage(string message)
            {
                if (!_canvas.gameObject.activeSelf)
                {
                    _canvas.gameObject.SetActive(true);
                }
                _roomInfoText.text = message;
                _playNowButton.interactable = false;
            }

            public void SetWaitText(int playersToWait)
            {
                if (!_isValid)
                {
                    return;
                }
                _roomInfoText.text = playersToWait == 1
                    ? $"Waiting for {playersToWait} player"
                    : $"Waiting for {playersToWait} players";
            }

            public Action PlayNowButtonOnClick
            {
                set
                {
                    if (!_isValid)
                    {
                        return;
                    }
                    _playNowButton.onClick.AddListener(() => { value(); });
                }
            }

            public IEnumerator Blink(float visibleDuration, float hiddenDuration)
            {
                var delay1 = new WaitForSeconds(visibleDuration);
                var delay2 = new WaitForSeconds(hiddenDuration);
                while (_isValid)
                {
                    yield return delay1;
                    _roomInfoText.enabled = false;
                    yield return delay2;
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    _roomInfoText.enabled = true;
                }
            }
        }
    }
}