using System.Collections;
using System.Collections.Generic;
using Photon.Client;
using Photon.Realtime;
using UnityEngine;

namespace Altzone.Scripts.Lobby.Wrappers
{
    public class LobbyRoom
    {
        /// <summary>
        /// A reference to the RealtimeClient which is currently keeping the connection and state.
        /// </summary>
        public RealtimeClient RealtimeClient { get; set; }

        private Room _room;

        /// <summary>The name of a room. Unique identifier (per region and virtual appid) for a room/match.</summary>
        /// <remarks>The name can't be changed once it's set by the server.</remarks>
        public string Name => _room.Name;

        /// <summary>True if this is an offline room (special case for PUN).</summary>
        public bool IsOffline => _room.IsOffline;

        /// <summary>
        /// Defines if the room can be joined.
        /// </summary>
        /// <remarks>
        /// This does not affect listing in a lobby but joining the room will fail if not open.
        /// If not open, the room is excluded from random matchmaking.
        /// Due to racing conditions, found matches might become closed while users are trying to join.
        /// Simply re-connect to master and find another.
        /// Use property "IsVisible" to not list the room.
        ///
        /// As part of RoomInfo this can't be set.
        /// As part of a Room (which the player joined), the setter will update the server and all clients.
        /// </remarks>
        public bool IsOpen => _room.IsOpen;

        /// <summary>
        /// Defines if the room is listed in its lobby.
        /// </summary>
        /// <remarks>
        /// Rooms can be created invisible, or changed to invisible.
        /// To change if a room can be joined, use property: open.
        ///
        /// As part of RoomInfo this can't be set.
        /// As part of a Room (which the player joined), the setter will update the server and all clients.
        /// </remarks>
        public bool IsVisible => _room.IsVisible;

        /// <summary>
        /// Sets a limit of players to this room. This property is synced and shown in lobby, too.
        /// If the room is full (players count == maxplayers), joining this room will fail.
        /// </summary>
        /// <remarks>
        /// As part of RoomInfo this can't be set.
        /// As part of a Room (which the player joined), the setter will update the server and all clients.
        /// </remarks>
        public int MaxPlayers => _room.MaxPlayers;

        /// <summary>The count of players in this Room (using this.Players.Count).</summary>
        public int PlayerCount => _room.PlayerCount;

        /// <summary>While inside a Room, this is the list of players who are also in that room.</summary>
        public Dictionary<int, Player> Players => _room.Players;

        /// <summary>
        /// List of users who are expected to join this room. In matchmaking, Photon blocks a slot for each of these UserIDs out of the MaxPlayers.
        /// </summary>
        /// <remarks>
        /// The corresponding feature in Photon is called "Slot Reservation" and can be found in the doc pages.
        /// Define expected players in the methods: <see cref="RealtimeClient.OpCreateRoom"/>, <see cref="RealtimeClient.OpJoinRoom"/> and <see cref="RealtimeClient.OpJoinRandomRoom"/>.
        /// </remarks>
        public string[] ExpectedUsers => _room.ExpectedUsers;

        /// <summary>Player Time To Live. How long any player can be inactive (due to disconnect or leave) before the user gets removed from the player list (freeing a slot).</summary>
        /// /// <remarks>If room.isOffline is true, no property will be set and there is no property-changed callback.</remarks>
        public int PlayerTtl => _room.PlayerTtl;

        /// <summary>Room Time To Live. How long a room stays available (and in server-memory), after the last player becomes inactive. After this time, the room gets persisted or destroyed.</summary>
        /// <remarks>If room.isOffline is true, no property will be set and there is no property-changed callback.</remarks>
        public int EmptyRoomTtl => _room.EmptyRoomTtl;

        /// <summary>
        /// The ID (actorNumber, actorNumber) of the player who's the master of this Room.
        /// Note: This changes when the current master leaves the room.
        /// </summary>
        public int MasterClientId => _room.MasterClientId;

        /// <summary>
        /// Gets a list of custom properties that are in the RoomInfo of the Lobby.
        /// This list is defined when creating the room and can't be changed afterwards. Compare: RealtimeClient.OpCreateRoom()
        /// </summary>
        /// <remarks>You could name properties that are not set from the beginning. Those will be synced with the lobby when added later on.</remarks>
        public object[] PropertiesListedInLobby => _room.PropertiesListedInLobby;

        /// <summary>
        /// Gets if this room cleans up the event cache when a player (actor) leaves.
        /// </summary>
        /// <remarks>
        /// This affects which events joining players get.
        ///
        /// Set in room creation via RoomOptions.CleanupCacheOnLeave.
        ///
        /// Within PUN, auto cleanup of events means that cached RPCs and instantiated networked objects are deleted from the room.
        /// </remarks>
        public bool AutoCleanUp => _room.AutoCleanUp;

        /// <summary>Define if the client who calls SetProperties should receive the properties update event or not. </summary>
        public bool BroadcastPropertiesChangeToAll => _room.BroadcastPropertiesChangeToAll;
        /// <summary>Define if Join and Leave events should not be sent to clients in the room. </summary>
        public bool SuppressRoomEvents => _room.SuppressRoomEvents;
        /// <summary>Extends SuppressRoomEvents: Define if Join and Leave events but also the actors' list and their respective properties should not be sent to clients. </summary>
        public bool SuppressPlayerInfo => _room.SuppressPlayerInfo;
        /// <summary>Define if UserIds of the players are broadcast in the room. Useful for FindFriends and reserving slots for expected users.</summary>
        public bool PublishUserId => _room.PublishUserId;
        /// <summary>Define if actor or room properties with null values are removed on the server or kept.</summary>
        public bool DeleteNullProperties => _room.DeleteNullProperties;
        /// <summary>The room's lobby (derived/tracked by the parameters of join, join random and create room operations).</summary>
        public TypedLobby Lobby => _room.Lobby;

#if SERVERSDK
        /// <summary>Define if rooms should have unique UserId per actor and that UserIds are used instead of actor number in rejoin.</summary>
        public bool CheckUserOnJoin => _room.CheckUserOnJoin;
#endif


        /// <summary>Creates a Room (representation) with given name and properties and the "listing options" as provided by parameters.</summary>
        /// <param name="roomName">Name of the room (can be null until it's actually created on server).</param>
        /// <param name="options">Room options.</param>
        /// <param name="isOffline">True when using the special case for offline rooms in PUN.</param>
        public LobbyRoom(string roomName, RoomOptions options, bool isOffline = false)
        {
            _room = new Room(roomName, options, isOffline);
        }

        public LobbyRoom(Room room)
        {
            _room = room;
        }

        /// <summary>
        /// Updates and synchronizes this Room's Custom Properties. Optionally, expectedProperties can be provided as condition.
        /// </summary>
        /// <remarks>
        /// Custom Properties are a set of string keys and arbitrary values which is synchronized
        /// for the players in a Room. They are available when the client enters the room, as
        /// they are in the response of OpJoin and OpCreate.
        ///
        /// Custom Properties either relate to the (current) Room or a Player (in that Room).
        ///
        /// Both classes locally cache the current key/values and make them available as
        /// property: CustomProperties. This is provided only to read them.
        /// You must use the method SetCustomProperties to set/modify them.
        ///
        /// Any client can set any Custom Properties anytime (when in a room).
        /// It's up to the game logic to organize how they are best used.
        ///
        /// You should call SetCustomProperties only with key/values that are new or changed. This reduces
        /// traffic and performance.
        ///
        /// Unless you define some expectedProperties, setting key/values is always permitted.
        /// In this case, the property-setting client will not receive the new values from the server but
        /// instead update its local cache in SetCustomProperties.
        ///
        /// If you define expectedProperties, the server will skip updates if the server property-cache
        /// does not contain all expectedProperties with the same values.
        /// In this case, the property-setting client will get an update from the server and update it's
        /// cached key/values at about the same time as everyone else.
        ///
        /// The benefit of using expectedProperties can be only one client successfully sets a key from
        /// one known value to another.
        /// As example: Store who owns an item in a Custom Property "ownedBy". It's 0 initally.
        /// When multiple players reach the item, they all attempt to change "ownedBy" from 0 to their
        /// actorNumber. If you use expectedProperties {"ownedBy", 0} as condition, the first player to
        /// take the item will have it (and the others fail to set the ownership).
        ///
        /// Properties get saved with the game state for Turnbased games (which use IsPersistent = true).
        /// </remarks>
        /// <param name="propertiesToSet">PhotonHashtable of Custom Properties that changes.</param>
        /// <param name="expectedValues">Provide some keys/values to use as condition for setting the new values. Client must be in room.</param>
        /// <returns>
        /// False if propertiesToSet is null or empty or have no keys (of allowed types).
        /// True in offline mode even if expectedProperties are used.
        /// Otherwise, returns if this operation could be sent to the server.
        /// </returns>
        public virtual bool SetCustomProperties(LobbyPhotonHashtable propertiesToSet, LobbyPhotonHashtable expectedValues = null)
        {
            if (expectedValues != null)
            {
                return _room.SetCustomProperties(propertiesToSet.GetOriginal(), expectedValues.GetOriginal());
            }
            else
            {
                return _room.SetCustomProperties(propertiesToSet.GetOriginal());
            }
        }

        /// <summary>
        /// Enables you to define the properties available in the lobby if not all properties are needed to pick a room.
        /// </summary>
        /// <remarks>
        /// Limit the amount of properties sent to users in the lobby to improve speed and stability.
        /// </remarks>
        /// <param name="lobbyProps">An array of custom room property names to forward to the lobby.</param>
        /// <returns>If the operation could be sent to the server.</returns>
        public bool SetPropertiesListedInLobby(object[] lobbyProps)
        {
            return _room.SetPropertiesListedInLobby(lobbyProps);
        }

        /// <summary>
        /// Asks the server to assign another player as Master Client of your current room.
        /// </summary>
        /// <remarks>
        /// RaiseEvent has the option to send messages only to the Master Client of a room.
        /// SetMasterClient affects which client gets those messages.
        ///
        /// This method calls an operation on the server to set a new Master Client, which takes a roundtrip.
        /// In case of success, this client and the others get the new Master Client from the server.
        ///
        /// SetMasterClient tells the server which current Master Client should be replaced with the new one.
        /// It will fail, if anything switches the Master Client moments earlier. There is no callback for this
        /// error. All clients should get the new Master Client assigned by the server anyways.
        ///
        /// See also: MasterClientId
        /// </remarks>
        /// <param name="masterClientPlayer">The player to become the next Master Client.</param>
        /// <returns>False when this operation couldn't be done currently. Requires a v4 Photon Server.</returns>
        public bool SetMasterClient(LobbyPlayer masterClientPlayer)
        {
            return _room.SetMasterClient(masterClientPlayer.GetPlayer(masterClientPlayer.ActorNumber));
        }

        /// <summary>
        /// Checks if the player is in the room's list already and calls StorePlayer() if not.
        /// </summary>
        /// <param name="player">The new player - identified by ID.</param>
        /// <returns>False if the player could not be added (cause it was in the list already).</returns>
        public virtual bool AddPlayer(LobbyPlayer player)
        {
            return _room.AddPlayer(player.GetPlayer(player.ActorNumber));
        }

        /// <summary>
        /// Updates a player reference in the Players dictionary (no matter if it existed before or not).
        /// </summary>
        /// <param name="player">The Player instance to insert into the room.</param>
        public virtual LobbyPlayer StorePlayer(LobbyPlayer player)
        {
            return new(_room.StorePlayer(player.GetPlayer(player.ActorNumber)));
        }

        /// <summary>
        /// Tries to find the player with given actorNumber (a.k.a. ID).
        /// Only useful when in a Room, as IDs are only valid per Room.
        /// </summary>
        /// <param name="id">ID to look for.</param>
        /// <param name="findMaster">If true, the Master Client is returned for ID == 0.</param>
        /// <returns>The player with the ID or null.</returns>
        public virtual LobbyPlayer GetPlayer(int id, bool findMaster = false)
        {
            return new(_room.GetPlayer(id, findMaster));
        }

        /// <summary>
        /// Attempts to remove all current expected users from the server's Slot Reservation list.
        /// </summary>
        /// <remarks>
        /// Note that this operation can conflict with new/other users joining. They might be
        /// adding users to the list of expected users before or after this client called ClearExpectedUsers.
        ///
        /// This room's expectedUsers value will update, when the server sends a successful update.
        ///
        /// Internals: This methods wraps up setting the ExpectedUsers property of a room.
        /// </remarks>
        /// <returns>If the operation could be sent to the server.</returns>
        public bool ClearExpectedUsers()
        {
            return _room.ClearExpectedUsers();
        }

        /// <summary>
        /// Attempts to update the expected users from the server's Slot Reservation list.
        /// </summary>
        /// <remarks>
        /// Note that this operation can conflict with new/other users joining. They might be
        /// adding users to the list of expected users before or after this client called SetExpectedUsers.
        ///
        /// This room's expectedUsers value will update, when the server sends a successful update.
        ///
        /// Internals: This methods wraps up setting the ExpectedUsers property of a room.
        /// </remarks>
        /// <param name="newExpectedUsers">The new array of UserIDs to be reserved in the room.</param>
        /// <returns>If the operation could be sent to the server.</returns>
        public bool SetExpectedUsers(string[] newExpectedUsers)
        {
            return _room.SetExpectedUsers(newExpectedUsers);
        }

        /// <summary>Returns a summary of this Room instance as string.</summary>
        /// <returns>Summary of this Room instance.</returns>
        public override string ToString()
        {
            return _room.ToString();
        }

        /// <summary>Returns a summary of this Room instance as longer string, including Custom Properties.</summary>
        /// <returns>Summary of this Room instance.</returns>
        public string ToStringFull()
        {
            return _room.ToStringFull();
        }

        public T GetCustomProperty<T>(string value, T defaultValue = default) => _room.GetCustomProperty<T>(value);
    }
}
