using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Static helper class to handle basic <c>PhotonNetwork</c> operations in convenient and consistent way.
    /// </summary>
    /// <remarks>
    /// Note (1) that by default we use <code>PhotonNetwork.AutomaticallySyncScene = true</code>.<br />
    /// Note (2) that <c>PhotonNetwork</c> <c>OfflineMode</c> must be handled separately if needed.
    /// </remarks>
    public static class PhotonLobby
    {
        private const int MaxRoomNameLength = 16;

        private static bool _isApplicationQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            GetGameVersion = () => DefaultGameVersion;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            _isApplicationQuitting = false;
            Application.quitting += () => _isApplicationQuitting = true;
        }

        /// <summary>
        /// Official game version for Photon (with Android bundle version code).
        /// </summary>
        public static string GameVersion => $"{GetGameVersion()}{BundleVersionCode.Get}";

        /// <summary>
        /// To override default <c>PhotonNetwork.GameVersion</c> (that is alias for <c>Application.version</c>).
        /// </summary>
        public static Func<string> GetGameVersion = () => DefaultGameVersion;

        private static string DefaultGameVersion => Application.version;

        public static void Connect(string playerName)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            if (PhotonNetwork.OfflineMode)
            {
                throw new UnityException("PhotonNetwork.OfflineMode not allowed");
            }
            if (!PhotonWrapper.CanConnect)
            {
                throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
            }
            var photonAppSettings = Resources.Load<PhotonAppSettings>(nameof(PhotonAppSettings));
            var appSettings = photonAppSettings != null ? photonAppSettings.appSettings : null;
            ConnectUsingSettings(appSettings, playerName);
        }

        public static void Disconnect()
        {
            PhotonNetwork.Disconnect();
        }

        public static void JoinLobby()
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            if (PhotonNetwork.OfflineMode)
            {
                throw new UnityException("PhotonNetwork.OfflineMode not allowed");
            }
            if (!PhotonWrapper.CanJoinLobby)
            {
                throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
            }
            Debug.Log(
                $"JoinLobby {PhotonNetwork.NetworkClientState} scene={SceneManager.GetActiveScene().name} GameVersion={PhotonNetwork.GameVersion}");
            PhotonNetwork.JoinLobby();
        }

        public static void LeaveLobby()
        {
            if (!PhotonNetwork.InLobby)
            {
                throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
            }
            Debug.Log("LeaveLobby");
            PhotonNetwork.LeaveLobby();
        }

        public static void CreateRoom(string roomName, RoomOptions roomOptions = null, bool isAutomaticallySyncScene = true)
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
            PhotonNetwork.AutomaticallySyncScene = isAutomaticallySyncScene;
            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
        }

        public static void JoinRoom(RoomInfo roomInfo, bool isAutomaticallySyncScene = true)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            Debug.Log($"JoinRoom {roomInfo.GetDebugLabel()} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
            PhotonNetwork.AutomaticallySyncScene = isAutomaticallySyncScene;
            var isJoined = PhotonNetwork.JoinRoom(roomInfo.Name);
            if (!isJoined)
            {
                Debug.LogWarning($"PhotonNetwork JoinRoom failed: {roomInfo.Name}");
            }
        }

        public static void JoinOrCreateRoom(string roomName,
            Hashtable customRoomProperties = null, string[] lobbyPropertyNames = null, bool isAutomaticallySyncScene = true)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            Debug.Log($"JoinOrCreateRoom {roomName} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
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
            var isSuccess = PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
            if (!isSuccess)
            {
                Debug.LogWarning($"PhotonNetwork JoinOrCreateRoom failed: {roomName}");
            }
        }

        public static void CloseRoom(bool keepVisible = false)
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

        public static void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                return;
            }
            throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
        }

        public static string GetRegion()
        {
            var region = PhotonNetwork.CloudRegion;
            if (region == null)
            {
                return string.Empty;
            }
            var index = region.IndexOf('/');
            if (index > 1)
            {
                region = region.Substring(0, index);
            }
            return region;
        }

        private static void ConnectUsingSettings(AppSettings appSettings, string playerName)
        {
            // See PhotonNetwork.SendRate (which is 30 times per sec)
            // https://documentation.help/Photon-Unity-Networking-2/class_photon_1_1_pun_1_1_photon_network.html#a7b4c9628657402e59fe292502511dcf4
            // - original 10 times per second is way too slow to keep moving objects synchronized properly without glitches!
            PhotonNetwork.SerializationRate = 30;
            Debug.Log(
                $"ConnectUsingSettings {PhotonNetwork.NetworkClientState} scene={SceneManager.GetActiveScene().name} player={playerName}" +
                $" {(appSettings != null ? appSettings.ToStringFull() : string.Empty)}");
            PhotonNetwork.NickName = playerName;
            PhotonNetwork.GameVersion = string.Empty;
            var started = appSettings != null
                ? PhotonNetwork.ConnectUsingSettings(appSettings)
                : PhotonNetwork.ConnectUsingSettings();
            if (started)
            {
                PhotonNetwork.GameVersion = GameVersion;
                Debug.Log($"Set GameVersion: {PhotonNetwork.GameVersion}");
            }
        }
    }
}