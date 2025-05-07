using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby.Wrappers;
using Photon.Client;
using Photon.Realtime;
using UnityEngine;

namespace Altzone.Scripts.Lobby.Wrappers
{
    public class LobbyPlayer
    {
        private Player _player;
        /// <summary>Identifier of this player in current room. Also known as: actorNumber or actorNumber. It's -1 outside of rooms.</summary>
        /// <remarks>The ID is assigned per room and only valid in that context. It will change even on leave and re-join. IDs are never re-used per room.</remarks>
        public int ActorNumber
        {
            get { return _player.ActorNumber; }
        }


        /// <summary>Only one player is controlled by each client. Others are not local.</summary>
        public bool IsLocal => _player.IsLocal;

        /// <summary>True if this player was inactive and did become active again.</summary>
        public bool HasRejoined => _player.HasRejoined;


        /// <summary>Nickname of this player. Non-unique and not authenticated. Synced automatically in a room.</summary>
        /// <remarks>
        /// A player might change his own nickname in a room (it's only a property).
        /// Setting this value updates the server and other players (using OpSetProperties internally).
        /// </remarks>
        public string NickName => _player.NickName;

        /// <summary>UserId of the player, available when the room got created with RoomOptions.PublishUserId = true.</summary>
        /// <remarks>Useful for <see cref="RealtimeClient.OpFindFriends"/> and blocking slots in a room for expected players (e.g. in <see cref="RealtimeClient.OpCreateRoom"/>).</remarks>
        public string UserId => _player.UserId;

        /// <summary>
        /// True if this player is the Master Client of the current room.
        /// </summary>
        public bool IsMasterClient => _player.IsMasterClient;

        /// <summary>If this player is active in the room (and getting events which are currently being sent).</summary>
        /// <remarks>
        /// Inactive players keep their spot in a room but otherwise behave as if offline (no matter what their actual connection status is).
        /// The room needs a PlayerTTL != 0. If a player is inactive for longer than PlayerTTL, the server will remove this player from the room.
        /// For a client "rejoining" a room, is the same as joining it: It gets properties, cached events and then the live events.
        /// </remarks>
        public bool IsInactive => _player.IsInactive;

        /// <summary>Read-only cache for custom properties of player. Set via Player.SetCustomProperties.</summary>
        /// <remarks>
        /// Don't modify the content of this PhotonHashtable. Use SetCustomProperties and the
        /// properties of this class to modify values. When you use those, the client will
        /// sync values with the server.
        /// </remarks>
        /// <see cref="SetCustomProperties"/>
        public LobbyPhotonHashtable CustomProperties
        {
            get
            {
                return new(_player.CustomProperties);
            }
            set
            {
                _player.CustomProperties = value.GetOriginal();
            }
        }

        /// <summary>Can be used to store a reference that's useful to know "by player".</summary>
        /// <remarks>Example: Set a player's character as Tag by assigning the GameObject on Instantiate.</remarks>
        public object TagObject => _player.TagObject;


        /// <summary>
        /// Creates a player instance.
        /// </summary>
        protected internal LobbyPlayer(Player player)
        {
            _player = player;
        }

        /// <summary>
        /// Get a Player by ActorNumber (Player.ID).
        /// </summary>
        /// <param name="id">ActorNumber of the a player in this room.</param>
        /// <returns>Player or null.</returns>
        public Player GetPlayer(int id)
        {
            return _player.Get(id);
        }

        /// <summary>
        /// Get a Player by ActorNumber (Player.ID).
        /// </summary>
        /// <param name="id">ActorNumber of the a player in this room.</param>
        /// <returns>Player or null.</returns>
        public LobbyPlayer GetLobbyPlayer(int id)
        {
            return new(_player.Get(id));
        }

        /// <summary>Gets this Player's next Player, as sorted by ActorNumber (Player.ID). Wraps around.</summary>
        /// <returns>Player or null.</returns>
        public LobbyPlayer GetNext()
        {
            return GetNextFor(_player.ActorNumber);
        }

        /// <summary>Gets a Player's next Player, as sorted by ActorNumber (Player.ID). Wraps around.</summary>
        /// <remarks>Useful when you pass something to the next player. For example: passing the turn to the next player.</remarks>
        /// <param name="currentPlayer">The Player for which the next is being needed.</param>
        /// <returns>Player or null.</returns>
        public LobbyPlayer GetNextFor(Player currentPlayer)
        {
            if (currentPlayer == null)
            {
                return null;
            }
            return GetNextFor(currentPlayer.ActorNumber);
        }

        /// <summary>Gets a Player's next Player, as sorted by ActorNumber (Player.ID). Wraps around.</summary>
        /// <remarks>Useful when you pass something to the next player. For example: passing the turn to the next player.</remarks>
        /// <param name="currentPlayerId">The ActorNumber (Player.ID) for which the next is being needed.</param>
        /// <returns>Player or null.</returns>
        public LobbyPlayer GetNextFor(int currentPlayerId)
        {
            return new(_player.GetNextFor(currentPlayerId));
        }

        public string GetDebugLabel(bool verbose = true) // Works for Room too!
        {
            return _player.GetDebugLabel(verbose);
        }

        /// <summary>
        /// Brief summary string of the Player: ActorNumber and NickName
        /// </summary>
        public override string ToString()
        {
            return string.Format("#{0:00} '{1}'", this.ActorNumber, this.NickName);
        }

        /// <summary>
        /// String summary of the Player: player.ID, name and all custom properties of this user.
        /// </summary>
        /// <remarks>
        /// Use with care and not every frame!
        /// Converts the customProperties to a String on every single call.
        /// </remarks>
        public string ToStringFull()
        {
            return string.Format("#{0:00} '{1}'{2} {3}", this.ActorNumber, this.NickName, this.IsInactive ? " (inactive)" : "", this.CustomProperties.ToStringFull());
        }

        /// <summary>
        /// If players are equal (by GetHasCode, which returns this.ID).
        /// </summary>
        public override bool Equals(object p)
        {
            LobbyPlayer pp = p as LobbyPlayer;
            return (pp != null && this.GetHashCode() == pp.GetHashCode());
        }

        /// <summary>
        /// Accompanies Equals, using the ID (actorNumber) as HashCode to return.
        /// </summary>
        public override int GetHashCode()
        {
            return this.ActorNumber;
        }


        /// <summary>
        /// Updates and synchronizes this Player's Custom Properties. Optionally, expectedProperties can be provided as condition.
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
        /// <param name="propertiesToSet">PhotonHashtable of Custom Properties to be set. </param>
        /// <param name="expectedValues">If non-null, these are the property-values the server will check as condition for this update.</param>
        /// <returns>
        /// False if propertiesToSet is null or empty or have no keys (of allowed types).
        /// True in offline mode even if expectedProperties are used.
        /// If not in a room, returns true if local player and expectedValues are null.
        /// (Use this to cache properties to be sent when joining a room).
        /// Otherwise, returns if this operation could be sent to the server.
        /// </returns>
        public bool SetCustomProperties(PhotonHashtable propertiesToSet, PhotonHashtable expectedValues = null)
        {
            return _player.SetCustomProperties(propertiesToSet, expectedValues);
        }

        public bool SetCustomProperties(LobbyPhotonHashtable propertiesToSet, LobbyPhotonHashtable expectedValues = null)
        {
            return _player.SetCustomProperties(propertiesToSet.GetOriginal(), expectedValues?.GetOriginal());
        }

        public bool HasCustomProperty( string key)
        {
            return _player.HasCustomProperty(key);
        }

        public T GetCustomProperty<T>(string key, T defaultValue = default)
        {
            return _player.GetCustomProperty(key, defaultValue);
        }
    }
}
