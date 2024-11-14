using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Client;
using Photon.Realtime;
using Prg.Scripts.Common.Util;
using Quantum;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PhotonRealtimeClient
{
    public static RealtimeClient Client;

    public static bool IsPhotonReady => Client.State == ClientState.PeerCreated ||
                                            Client.State == ClientState.Disconnected ||
                                            Client.State == ClientState.ConnectedToMasterServer;
    public static bool CanConnect => Client.State == ClientState.PeerCreated ||
                                         Client.State == ClientState.Disconnected;

    public static bool CanJoinLobby => Client.State == ClientState.ConnectedToMasterServer;

    public static PhotonServerSettings ServerSettings { get; private set; }

    public static string NickName
    {
        get
        {
            return Client.NickName;
        }

        set
        {
            Client.NickName = value;
        }
    }

    private static string DefaultGameVersion => $"{Application.version}.{_bundleVersion}";
    private static int _bundleVersion = 0;

    public static string GameVersion
    {
        get { return gameVersion; }
        set
        {
            gameVersion = value;
            Client.AppSettings.AppVersion = string.Format("{0}", value);
        }
    }
    private static string gameVersion;

    public static AuthenticationValues AuthValues
    {
        get { return (Client != null) ? Client.AuthValues : null; }
        set { if (Client != null) Client.AuthValues = value; }
    }

    public static int CountOfPlayers
    {
        get
        {
            return Client.PlayersInRoomsCount + Client.PlayersOnMasterCount;
        }
    }

    public static bool InLobby
    {
        get
        {
            return Client.InLobby;
        }
    }

    public static bool IsConnected
    {
        get
        {
            /*if (OfflineMode)
            {
                return true;
            }*/

            if (Client == null)
            {
                return false;
            }

            return Client.IsConnected;
        }
    }

    public static bool IsMessageQueueRunning
    {
        get
        {
            return isMessageQueueRunning;
        }

        set
        {
            isMessageQueueRunning = value;
        }
    }

    public static string CloudRegion { get { return (Client != null && Client.IsConnected && Client.Server != ServerConnection.NameServer) ? Client.CurrentRegion : null; } }


    private static bool isMessageQueueRunning = true;

    public static LogLevel LogLevel = LogLevel.Error;

    public static bool AutomaticallySyncScene
    {
        get
        {
            return automaticallySyncScene;
        }
        set
        {
            automaticallySyncScene = value;
            if (automaticallySyncScene && CurrentRoom != null)
            {
                LoadLevelIfSynced();
            }
        }
    }
    public static bool EnableCloseConnection = false;

    private static AsyncOperation _AsyncLevelLoadingOperation;

    private static float _levelLoadingProgress = 0f;

    public static float LevelLoadingProgress
    {
        get
        {
            if (_AsyncLevelLoadingOperation != null)
            {
                _levelLoadingProgress = _AsyncLevelLoadingOperation.progress;
            }
            else if (_levelLoadingProgress > 0f)
            {
                _levelLoadingProgress = 1f;
            }

            return _levelLoadingProgress;
        }
    }

    internal static bool loadingLevelAndPausedNetwork = false;

    private static bool automaticallySyncScene = false;

    public class PhotonEvent
    {
        public const byte StartGame = 110;
        public const byte RPC = 200;
        public const byte SendSerialize = 201;
        public const byte Instantiation = 202;
        public const byte CloseConnection = 203;
        public const byte Destroy = 204;
        public const byte RemoveCachedRPCs = 205;
        public const byte SendSerializeReliable = 206; // TS: added this but it's not really needed anymore
        public const byte DestroyPlayer = 207; // TS: added to make others remove all GOs of a player
        public const byte OwnershipRequest = 209;
        public const byte OwnershipTransfer = 210;
        public const byte VacantViewIds = 211;
        public const byte OwnershipUpdate = 212;
    }

    public static Player LocalPlayer
    {
        get
        {
            if (Client == null)
            {
                return null; // suppress ExitApplication errors
            }

            return Client.LocalPlayer;
        }
    }

    public static Player[] PlayerList
    {
        get
        {
            Room room = CurrentRoom;
            if (room != null)
            {
                // TODO: implement more effectively. maybe cache?!
                return room.Players.Values.OrderBy((x) => x.ActorNumber).ToArray();
            }
            return new Player[0];
        }
    }

    public static Player[] PlayerListOthers
    {
        get
        {
            Room room = CurrentRoom;
            if (room != null)
            {
                // TODO: implement more effectively. maybe cache?!
                return room.Players.Values.OrderBy((x) => x.ActorNumber).Where(x => !x.IsLocal).ToArray();
            }
            return new Player[0];
        }
    }

    public const string ServerSettingsFileName = "PhotonServerSettings";
    internal const string CurrentSceneProperty = "curScn";
    internal const string CurrentScenePropertyLoadAsync = "curScnLa";

    public static Room CurrentRoom
    {
        get
        {
            /*if (offlineMode)
            {
                return offlineModeRoom;
            }*/

            return Client == null ? null : Client.CurrentRoom;
        }
    }


    public static ClientState NetworkClientState
    {
        get
        {
            /*if (OfflineMode)
            {
                return (offlineModeRoom != null) ? ClientState.Joined : ClientState.ConnectedToMasterServer;
            }*/

            if (Client == null)
            {
                return ClientState.Disconnected;
            }

            return Client.State;
        }
    }

    static PhotonRealtimeClient()
    {
        #if !UNITY_EDITOR
            StartClient();  // in builds, we just reset/init the client once
        #else
            Client = new RealtimeClient();
        #endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void StartClient()
    {
        ServerSettings = PhotonServerSettings.Global;
        var protocol = ServerSettings.AppSettings.Protocol;
        Client = new RealtimeClient(protocol);
    }

    public static bool JoinLobby(TypedLobby typedLobby)
    {
        if (Client.IsConnected && Client.Server == ServerConnection.MasterServer)
        {
            return Client.OpJoinLobby(typedLobby);
        }

        return false;
    }

    public static bool LeaveLobby()
    {
        if (Client.IsConnected && Client.Server == ServerConnection.MasterServer)
        {
            return Client.OpLeaveLobby();
        }

        return false;
    }

    public static void Connect(string playerName, string regionCodeOverride = null)
    {
        /*if (_isApplicationQuitting)
        {
            return;
        }
        if (OfflineMode)
        {
            throw new UnityException("PhotonNetwork.OfflineMode not allowed");
        }*/
        if (!CanConnect)
        {
            throw new UnityException($"Invalid connection state: {NetworkClientState}");
        }
        if (string.IsNullOrWhiteSpace(playerName))
        {
            throw new UnityException("Player name is missing");
        }
        // We use explicit settings - there was a bug related to settings get corrupted earlier.
        var appSettings = PhotonServerSettings.Global.AppSettings;
        ConnectUsingSettings(playerName, appSettings, regionCodeOverride);
    }

    private static void ConnectUsingSettings(string playerName, AppSettings appSettings, string regionCodeOverride)
    {
        // Defaults are 30 times/second for SendRate and 10 times/second for SerializationRate, we set both explicitly here.
        //PhotonNetwork.SendRate = 30;
        //PhotonNetwork.SerializationRate = 30;
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
        NickName = playerName;
        GameVersion = string.Empty;
        var started = ConnectUsingSettings(appSettings);
        if (!started)
        {
            Debug.LogError(
                $"Failed to ConnectUsingSettings: state={NetworkClientState} appSettings={appSettings.ToStringFull()}");
            return;
        }
        Debug.LogWarning("Check2");
        // Set the GameVersion right after calling ConnectUsingSettings!
        GameVersion = DefaultGameVersion;
    }
    public static bool ConnectUsingSettings(AppSettings appSettings, bool startInOfflineMode = false) // parameter name hides static class member
    {
        if (Client.RealtimePeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: " + Client.RealtimePeer.PeerState);
            return false;
        }
        if (ConnectionHandler.AppQuits)
        {
            Debug.LogWarning("Can't connect: Application is closing. Unity called OnApplicationQuit().");
            return false;
        }
        if (PhotonServerSettings.Global == null)
        {
            Debug.LogError("Can't connect: Loading settings failed. ServerSettings asset must be in any 'Resources' folder as: " + ServerSettingsFileName);
            return false;
        }

        SetupLogging();


        Client.RealtimePeer.TransportProtocol = appSettings.Protocol;
        Client.AppSettings.EnableProtocolFallback = appSettings.EnableProtocolFallback;
        Client.AppSettings.AuthMode = appSettings.AuthMode;


        IsMessageQueueRunning = true;
        //Client.AppSettings.AppIdRealtime = appSettings.AppIdQuantum;
        Client.AppSettings.AppIdQuantum = appSettings.AppIdQuantum;
        GameVersion = appSettings.AppVersion;



        /*if (startInOfflineMode)
        {
            OfflineMode = true;
            return true;
        }

        if (OfflineMode)
        {
            OfflineMode = false; // Cleanup offline mode
                                 // someone can set OfflineMode in code and then call ConnectUsingSettings() with non-offline settings. Warning for that case:
            Debug.LogWarning("ConnectUsingSettings() disabled the offline mode. No longer offline.");
        }*/


        Client.AppSettings.EnableLobbyStatistics = appSettings.EnableLobbyStatistics;
        Client.AppSettings.ProxyServer = appSettings.ProxyServer;


        if (appSettings.IsMasterServerAddress)
        {
            if (AuthValues == null)
            {
                AuthValues = new AuthenticationValues(Guid.NewGuid().ToString());
            }
            else if (string.IsNullOrEmpty(AuthValues.UserId))
            {
                AuthValues.UserId = Guid.NewGuid().ToString();
            }
            return ConnectToMaster(appSettings.Server, appSettings.Port, appSettings.AppIdQuantum);
        }


        Client.AppSettings.Port = appSettings.Port;
        if (!appSettings.IsDefaultNameServer)
        {
            Client.NameServerHost = appSettings.Server;
        }


        if (appSettings.IsBestRegion)
        {
            Debug.LogWarning("Check1");
            return ConnectToBestCloudServer();
        }

        return ConnectToRegion(appSettings.FixedRegion);
    }

    public static bool ConnectToMaster(string masterServerAddress, int port, string appID)
    {
        // TODO: refactor NetworkingClient.LoadBalancingPeer.PeerState to not use the peer but LBC.connected or so
        if (Client.RealtimePeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("ConnectToMaster() failed. Can only connect while in state 'Disconnected'. Current state: " + Client.RealtimePeer.PeerState);
            return false;
        }
        if (ConnectionHandler.AppQuits)
        {
            Debug.LogWarning("Can't connect: Application is closing. Unity called OnApplicationQuit().");
            return false;
        }

        /*if (OfflineMode)
        {
            OfflineMode = false; // Cleanup offline mode
            Debug.LogWarning("ConnectToMaster() disabled the offline mode. No longer offline.");
        }*/

        if (!IsMessageQueueRunning)
        {
            IsMessageQueueRunning = true;
            Debug.LogWarning("ConnectToMaster() enabled IsMessageQueueRunning. Needs to be able to dispatch incoming messages.");
        }

        SetupLogging();
        //ConnectMethod = ConnectMethod.ConnectToMaster;

        Client.AppSettings.UseNameServer = false;
        Client.MasterServerAddress = (port == 0) ? masterServerAddress : masterServerAddress + ":" + port;
        Client.AppSettings.AppIdQuantum = appID;

        return Client.ConnectUsingSettings(Client.AppSettings);
    }

    public static bool ConnectToBestCloudServer()
    {
        if (Client.RealtimePeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("ConnectToBestCloudServer() failed. Can only connect while in state 'Disconnected'. Current state: " + Client.RealtimePeer.PeerState);
            return false;
        }
        if (ConnectionHandler.AppQuits)
        {
            Debug.LogWarning("Can't connect: Application is closing. Unity called OnApplicationQuit().");
            return false;
        }

        SetupLogging();
        //ConnectMethod = ConnectMethod.ConnectToBest;

        // Connecting to "Best Region" begins with connecting to the Name Server.
        bool couldConnect = Client.ConnectUsingSettings(Client.AppSettings);
        return couldConnect;
    }

    public static bool ConnectToRegion(string region)
    {
        if (Client.RealtimePeer.PeerState != PeerStateValue.Disconnected && Client.Server != ServerConnection.NameServer)
        {
            Debug.LogWarning("ConnectToRegion() failed. Can only connect while in state 'Disconnected'. Current state: " + Client.RealtimePeer.PeerState);
            return false;
        }
        if (ConnectionHandler.AppQuits)
        {
            Debug.LogWarning("Can't connect: Application is closing. Unity called OnApplicationQuit().");
            return false;
        }

        SetupLogging();
        //ConnectMethod = ConnectMethod.ConnectToRegion;

        Client.AppSettings.FixedRegion = region;

        if (!string.IsNullOrEmpty(region))
        {
            return Client.ConnectUsingSettings(Client.AppSettings);
        }

        return false;
    }

    public static void Disconnect()
    {
        /*if (OfflineMode)
        {
            OfflineMode = false;
            offlineModeRoom = null;
            NetworkingClient.State = ClientState.Disconnecting;
            NetworkingClient.OnStatusChanged(StatusCode.Disconnect);
            return;
        }*/

        if (Client == null)
        {
            return; // Surpress error when quitting playmode in the editor
        }

        Client.Disconnect();
    }

    public static bool CreateRoom(string roomName, RoomOptions roomOptions = null, TypedLobby typedLobby = null, string[] expectedUsers = null)
    {
        /*if (OfflineMode)
        {
            if (offlineModeRoom != null)
            {
                Debug.LogError("CreateRoom failed. In offline mode you still have to leave a room to enter another.");
                return false;
            }
            EnterOfflineRoom(roomName, roomOptions, true);
            return true;
        }*/
        if (Client.Server != ServerConnection.MasterServer || !Client.IsConnectedAndReady)
        {
            Debug.LogError("CreateRoom failed. Client is on " + Client.Server + " (must be Master Server for matchmaking)" + (Client.IsConnectedAndReady ? " and ready" : "but not ready for operations (State: " + Client.State + ")") + ". Wait for callback: OnJoinedLobby or OnConnectedToMaster.");
            return false;
        }

        typedLobby ??= ((Client.InLobby) ? Client.CurrentLobby : null);  // use given lobby, or active lobby (if any active) or none

        EnterRoomArgs opParams = new EnterRoomArgs();
        opParams.RoomName = roomName;
        opParams.RoomOptions = roomOptions;
        opParams.Lobby = typedLobby;
        opParams.ExpectedUsers = expectedUsers;

        return Client.OpCreateRoom(opParams);
    }

    public static bool JoinRoom(string roomName, string[] expectedUsers = null)
    {
        /*if (OfflineMode)
        {
            if (offlineModeRoom != null)
            {
                Debug.LogError("JoinRoom failed. In offline mode you still have to leave a room to enter another.");
                return false;
            }
            EnterOfflineRoom(roomName, null, true);
            return true;
        }*/
        if (Client.Server != ServerConnection.MasterServer || !Client.IsConnectedAndReady)
        {
            Debug.LogError("JoinRoom failed. Client is on " + Client.Server + " (must be Master Server for matchmaking)" + (Client.IsConnectedAndReady ? " and ready" : "but not ready for operations (State: " + Client.State + ")") + ". Wait for callback: OnJoinedLobby or OnConnectedToMaster.");
            return false;
        }
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("JoinRoom failed. A roomname is required. If you don't know one, how will you join?");
            return false;
        }


        EnterRoomArgs opParams = new EnterRoomArgs();
        opParams.RoomName = roomName;
        opParams.ExpectedUsers = expectedUsers;

        return Client.OpJoinRoom(opParams);
    }

    public static bool LeaveRoom(bool becomeInactive = true)
    {
        /*if (OfflineMode)
        {
            offlineModeRoom = null;
            Client.MatchMakingCallbackTargets.OnLeftRoom();
            Client.ConnectionCallbackTargets.OnConnectedToMaster();
        }
        else
        {*/
            if (CurrentRoom == null)
            {
                Debug.LogWarning("PhotonNetwork.CurrentRoom is null. You don't have to call LeaveRoom() when you're not in one. State: " + NetworkClientState);
            }
            else
            {
                becomeInactive = becomeInactive && CurrentRoom.PlayerTtl != 0; // in a room with playerTTL == 0, the operation "leave" will never turn a client inactive
            }
            return Client.OpLeaveRoom(becomeInactive);
        //}

        //return true;
    }

    public static void CloseRoom(bool keepVisible = false)
    {
        if (!Client.InRoom)
        {
            throw new UnityException($"Invalid connection state: {NetworkClientState}");
        }
        if (!LocalPlayer.IsMasterClient)
        {
            throw new UnityException($"Player is not Master Client: {LocalPlayer.GetDebugLabel()}");
        }
        var room = CurrentRoom;
        if (!room.IsOpen)
        {
            throw new UnityException($"Room is closed already: {room.GetDebugLabel()}");
        }
        room.IsOpen = false;
        room.IsVisible = keepVisible;
    }

    public static bool CloseConnection(Player kickPlayer)
    {
        if (!VerifyCanUseNetwork())
        {
            return false;
        }

        if (!EnableCloseConnection)
        {
            Debug.LogError("CloseConnection is disabled. No need to call it.");
            return false;
        }

        if (!LocalPlayer.IsMasterClient)
        {
            Debug.LogError("CloseConnection: Only the masterclient can kick another player.");
            return false;
        }

        if (kickPlayer == null)
        {
            Debug.LogError("CloseConnection: No such player connected!");
            return false;
        }

        RaiseEventArgs options = new RaiseEventArgs() { TargetActors = new int[] { kickPlayer.ActorNumber } };
        return Client.OpRaiseEvent(PhotonEvent.CloseConnection, null, options, SendOptions.SendReliable);
    }

    private static void SetupLogging()
    {
        // only apply Settings if LogLevel is default ( see ServerSettings.cs), else it means it's been set programmatically
        if (LogLevel == LogLevel.Error)
        {
            LogLevel = PhotonServerSettings.Global.AppSettings.ClientLogging;
        }

        // only apply Settings if LogLevel is default ( see ServerSettings.cs), else it means it's been set programmatically
        if (Client.RealtimePeer.LogLevel == LogLevel.Error)
        {
            Client.RealtimePeer.LogLevel = PhotonServerSettings.Global.AppSettings.NetworkLogging;
        }
    }

    internal static void LoadLevelIfSynced()
    {
        if (!AutomaticallySyncScene || Client.LocalPlayer.IsMasterClient || Client.CurrentRoom == null)
        {
            return;
        }

        // check if "current level" is set in props
        if (!Client.CurrentRoom.CustomProperties.ContainsKey(CurrentSceneProperty))
        {
            return;
        }

        // if loaded level is not the one defined by master in props, load that level
        object sceneId = Client.CurrentRoom.CustomProperties[CurrentSceneProperty];
        if (sceneId is int)
        {
            if (SceneManager.GetActiveScene().buildIndex != (int)sceneId)
            {
                LoadLevel((int)sceneId);
            }
        }
        else if (sceneId is string)
        {
            if (SceneManager.GetActiveScene().name != (string)sceneId)
            {
                LoadLevel((string)sceneId);
            }
        }
    }

    public static void LoadLevel(int levelNumber)
    {
        if (ConnectionHandler.AppQuits)
        {
            return;
        }

        if (AutomaticallySyncScene)
        {
            //SetLevelInPropsIfSynced(levelNumber);
        }

        IsMessageQueueRunning = false;
        loadingLevelAndPausedNetwork = true;
        _AsyncLevelLoadingOperation = SceneManager.LoadSceneAsync(levelNumber, LoadSceneMode.Single);
    }

    public static void LoadLevel(string levelName)
    {
        if (ConnectionHandler.AppQuits)
        {
            return;
        }

        if (AutomaticallySyncScene)
        {
            //SetLevelInPropsIfSynced(levelName);
        }

        IsMessageQueueRunning = false;
        loadingLevelAndPausedNetwork = true;
        _AsyncLevelLoadingOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
    }

    private static bool VerifyCanUseNetwork()
    {
        if (IsConnected)
        {
            return true;
        }

        Debug.LogError("Cannot send messages when not connected. Either connect to Photon OR use offline mode!");
        return false;
    }

    public static long GetPing()
    {
        return Client.RealtimePeer.Stats.RoundtripTime;
    }
}
