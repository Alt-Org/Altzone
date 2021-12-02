using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Static helper class to handle basic <c>PhotonNetwork</c> operations in convenient and consistent way.
    /// </summary>
    public static class PhotonLobby
    {
        private const int MaxRoomNameLength = 16;

        private static bool _isApplicationQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
            Application.quitting += () => _isApplicationQuitting = true;
        }

        /// <summary>
        /// Official game version for Photon.
        /// </summary>
        public static string gameVersion => _gameVersion();

        /// <summary>
        /// To override default <c>PhotonNetwork.GameVersion</c> that is alias for <c>Application.version</c>.
        /// </summary>
        public static Func<string> _gameVersion = () => Application.version;

        public static bool OfflineMode
        {
            get => PhotonNetwork.OfflineMode;
            set => PhotonNetwork.OfflineMode = value;
        }

        public static void connect(string playerName, bool isAutomaticallySyncScene = true)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            if (PhotonNetwork.OfflineMode)
            {
                throw new UnityException("PhotonNetwork.OfflineMode not allowed");
            }
            if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
            {
                var photonAppSettings = ResourceLoader.Get().LoadAsset<PhotonAppSettings>(nameof(PhotonAppSettings));
                var appSettings = photonAppSettings != null ? photonAppSettings.appSettings : null;
                ConnectUsingSettings(appSettings, playerName, isAutomaticallySyncScene);
                return;
            }
            throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
        }

        public static void disconnect()
        {
            PhotonNetwork.Disconnect();
        }

        public static void joinLobby()
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            if (PhotonNetwork.OfflineMode)
            {
                throw new UnityException("PhotonNetwork.OfflineMode not allowed");
            }
            if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
            {
                Debug.Log(
                    $"JoinLobby {PhotonNetwork.NetworkClientState} scene={SceneManager.GetActiveScene().name} GameVersion={PhotonNetwork.GameVersion}");
                PhotonNetwork.JoinLobby();
                return;
            }
            throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
        }

        public static void leaveLobby()
        {
            if (PhotonNetwork.InLobby)
            {
                Debug.Log("LeaveLobby");
                PhotonNetwork.LeaveLobby();
                return;
            }
            throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
        }

        public static void createRoom(string roomName, RoomOptions roomOptions = null)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = null; // Let Photon generate room name us to ensure that room creation succeeds
            }
            else if (roomName.Length > MaxRoomNameLength)
            {
                roomName = roomName.Substring(0, MaxRoomNameLength);
            }
            var options = roomOptions ?? new RoomOptions
            {
                IsOpen = true,
                IsVisible = true
            };
            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
        }

        public static bool joinRoom(RoomInfo roomInfo)
        {
            if (_isApplicationQuitting)
            {
                return false;
            }
            Debug.Log($"JoinRoom {roomInfo.GetDebugLabel()} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
            var isJoined = PhotonNetwork.JoinRoom(roomInfo.Name);
            if (!isJoined)
            {
                Debug.LogWarning("PhotonNetwork JoinRoom failed");
            }
            return isJoined;
        }

        public static void joinOrCreateRoom(string roomName,
            Hashtable customRoomProperties = null, string[] lobbyPropertyNames = null, bool isAutomaticallySyncScene = false)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            Debug.Log($"joinOrCreateRoom {roomName}");
            if (string.IsNullOrWhiteSpace(roomName))
            {
                throw new UnityException("roomName can not be null or empty");
            }
            var options = new RoomOptions
            {
                CustomRoomProperties = customRoomProperties,
                CustomRoomPropertiesForLobby = lobbyPropertyNames
            };
            PhotonNetwork.AutomaticallySyncScene = isAutomaticallySyncScene;
            PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
        }

        public static void closeRoom(bool keepVisible = false)
        {
            if (!PhotonNetwork.InRoom)
            {
                throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
            }
            if (!PhotonNetwork.IsMasterClient)
            {
                throw new UnityException($"Player is not Master Client: {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
            }
            var room = PhotonNetwork.CurrentRoom;
            if (!room.IsOpen)
            {
                throw new UnityException($"Room is closed already: {room.GetDebugLabel()}");
            }
            room.IsOpen = false;
            room.IsVisible = keepVisible;
        }

        public static void leaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                return;
            }
            throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
        }

        private static void ConnectUsingSettings(AppSettings appSettings, string playerName, bool isAutomaticallySyncScene)
        {
            // See PhotonNetwork.SendRate (which is 30 times per sec)
            // https://documentation.help/Photon-Unity-Networking-2/class_photon_1_1_pun_1_1_photon_network.html#a7b4c9628657402e59fe292502511dcf4
            // - original 10 times per second is way too slow to keep moving objects synchronized properly without glitches!
            PhotonNetwork.SerializationRate = 30;
            Debug.Log(
                $"ConnectUsingSettings {PhotonNetwork.NetworkClientState} scene={SceneManager.GetActiveScene().name} player={playerName}" +
                $" {(appSettings != null ? appSettings.ToStringFull() : "")}");
            PhotonNetwork.AutomaticallySyncScene = isAutomaticallySyncScene;
            PhotonNetwork.NickName = playerName;
            PhotonNetwork.GameVersion = "";
            var started = appSettings != null
                ? PhotonNetwork.ConnectUsingSettings(appSettings)
                : PhotonNetwork.ConnectUsingSettings();
            if (started)
            {
                PhotonNetwork.GameVersion = gameVersion;
                Debug.Log($"Set GameVersion: {PhotonNetwork.GameVersion}");
            }
        }
    }
}