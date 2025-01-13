using System;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Prg.Scripts.Common.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
/*using AppSettings = Battle1.PhotonRealtime.Code.AppSettings;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using RoomInfo = Battle1.PhotonRealtime.Code.RoomInfo;
using RoomOptions = Battle1.PhotonRealtime.Code.RoomOptions;
using TypedLobby = Battle1.PhotonRealtime.Code.TypedLobby;*/

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Imaginary interface for Photon Lobby (for comprehension of current implementation).
    /// </summary>
    public interface IPhotonLobby
    {
        /// <summary>
        /// Photon version management (client peer-to-peer connectivity).
        /// </summary>
        string GameVersion { get; }

        /// <summary>
        /// Connection management.
        /// </summary>
        void Connect(string playerName, string regionCodeOverride = null);

        void Disconnect();

        /// <summary>
        /// Lobby, required to join or create room.
        /// </summary>
        void JoinLobby();

        void LeaveLobby();

        /// <summary>
        /// Room, required to play with others.
        /// </summary>
        void CreateRoom(string roomName, RoomOptions roomOptions = null, bool isAutomaticallySyncScene = true);

        void JoinRoom(RoomInfo roomInfo, bool isAutomaticallySyncScene = true);

        /*void JoinOrCreateRoom(string roomName,
            Hashtable customRoomProperties = null, string[] lobbyPropertyNames = null,
            bool isAutomaticallySyncScene = true);*/

        void CloseRoom(bool keepVisible = false);
        void LeaveRoom();

        /// <summary>
        /// Photon game server region (for your information).
        /// </summary>
        string GetRegion();
    }

    /// <summary>
    /// Static helper class to handle basic <c>PhotonNetwork</c> operations in convenient and consistent way.<br />
    /// https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_pun_1_1_photon_network.html
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

        /*[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            _isApplicationQuitting = false;
            Application.quitting += () => _isApplicationQuitting = true;
        }*/

        /// <summary>
        /// Official game version used for Photon Connect (PhotonNetwork.GameVersion).
        /// </summary>
        public static string GameVersion => GetGameVersion();

        /// <summary>
        /// Sets actual bundle version from game compile time to be used in PhotonNetwork.GameVersion.
        /// </summary>
        public static void SetBundleVersion(int bundleVersion) => _bundleVersion = bundleVersion;

        /// <summary>
        /// To override default 'GameVersion' totally for development etc.
        /// </summary>
        public static Func<string> GetGameVersion = () => DefaultGameVersion;

        private static string DefaultGameVersion => $"{Application.version}.{_bundleVersion}";
        private static int _bundleVersion = 0;

        public static void Connect(string playerName, string regionCodeOverride = null)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            /*if (PhotonNetwork.OfflineMode)
            {
                throw new UnityException("PhotonNetwork.OfflineMode not allowed");
            }
            if (!PhotonWrapper.CanConnect)
            {
                throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
            }*/
            if (string.IsNullOrWhiteSpace(playerName))
            {
                throw new UnityException("Player name is missing");
            }
            // We use explicit settings - there was a bug related to settings get corrupted earlier.
            /*var appSettings = PhotonNetwork.PhotonServerSettings.AppSettings;*/
            AppSettings appSettings = null;
            ConnectUsingSettings(playerName, appSettings, regionCodeOverride);
        }

        public static void Disconnect()
        {
            /*PhotonNetwork.Disconnect();*/
        }

        public static void JoinLobby()
        {
            if (_isApplicationQuitting)
            {
                return;
            }
       /*     if (PhotonNetwork.OfflineMode)
            {
                throw new UnityException("PhotonNetwork.OfflineMode not allowed");
            }
            if (!PhotonWrapper.CanJoinLobby)
            {
                throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
            }*/
           /* Debug.Log(
                $"JoinLobby {PhotonNetwork.NetworkClientState} scene={SceneManager.GetActiveScene().name} GameVersion={PhotonNetwork.GameVersion}");
            PhotonNetwork.JoinLobby();*/
        }

        public static void LeaveLobby()
        {
          /*  if (!PhotonNetwork.InLobby)
            {
                throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
            }
            Debug.Log("LeaveLobby");
            PhotonNetwork.LeaveLobby();*/
        }

        public static void CreateRoom(string roomName, RoomOptions roomOptions = null,
            bool isAutomaticallySyncScene = true)
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
            /*PhotonNetwork.AutomaticallySyncScene = isAutomaticallySyncScene;
            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);*/
        }

        /*public static void JoinRoom(RoomInfo roomInfo, bool isAutomaticallySyncScene = true)
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
        }*/

        /*public static void JoinOrCreateRoom(string roomName,
            Hashtable customRoomProperties = null, string[] lobbyPropertyNames = null,
            bool isAutomaticallySyncScene = true)
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
        }*/

   /*     public static void CloseRoom(bool keepVisible = false)
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
        }*/

        public static void LeaveRoom()
        {
          /*  if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                return;
            }*/
           /* throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");*/
        }

        public static string GetRegion()
        {
            /*  var region = PhotonNetwork.CloudRegion;
              if (region == null)
              {
                  return string.Empty;
              }
              var index = region.IndexOf('/');
              if (index > 1)
              {
                  region = region.Substring(0, index);
              }
              return region;*/
            return "IDK";
        }

        private static void ConnectUsingSettings(string playerName, AppSettings appSettings, string regionCodeOverride)
        {
            Debug.LogWarning("Photon PUN is not in use!");
            return;
            // See PhotonNetwork.SendRate and PhotonNetwork.SerializationRate
            // https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_pun_1_1_photon_network.html#a7b4c9628657402e59fe292502511dcf4
            // https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_pun_1_1_photon_transform_view.html
            // Note that PUN will also send data at the end of frames that wrote data in OnPhotonSerializeView!
            // This means that if you serialize data always when OnPhotonSerializeView is called
            // then SendRate will effectively be same as SerializationRate if it is set to be less here.

            // Defaults are 30 times/second for SendRate and 10 times/second for SerializationRate, we set both explicitly here.
           /* PhotonNetwork.SendRate = 30;
            PhotonNetwork.SerializationRate = 30;*/
            if (!string.IsNullOrEmpty(regionCodeOverride))
            {
#if UNITY_EDITOR
                // Create a copy so we do not change data we do not own.
                // This is (copy) quite slow but its ok here when connecting to game server takes time by itself.
                var instance = new AppSettings();
                PropertyCopier<AppSettings, AppSettings>.CopyFields(appSettings, instance);
                appSettings = instance;
#endif
                appSettings.FixedRegion = regionCodeOverride;
            }
            Debug.Log($"player={playerName} GameVersion={GameVersion} appSettings={appSettings.ToStringFull()}");
        /*    PhotonNetwork.NickName = playerName;
            PhotonNetwork.GameVersion = string.Empty;
            var started = PhotonNetwork.ConnectUsingSettings(appSettings);*/
            /*if (!started)
            {
                Debug.LogError(
                    $"Failed to ConnectUsingSettings: state={PhotonNetwork.NetworkClientState} appSettings={appSettings.ToStringFull()}");
                return;
            }
            // Set the GameVersion right after calling ConnectUsingSettings!
            PhotonNetwork.GameVersion = GameVersion;*/
        }
    }
}
