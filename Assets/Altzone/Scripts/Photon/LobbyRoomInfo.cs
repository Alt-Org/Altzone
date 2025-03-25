using System.Collections;
using System.Collections.Generic;
using Photon.Client;
using Photon.Realtime;
using UnityEngine;


namespace Altzone.Scripts.Lobby.Wrappers
{
    public class LobbyRoomInfo
    {
        private RoomInfo roomInfo;

        public bool RemovedFromList => roomInfo.RemovedFromList;

        /// <summary>Read-only "cache" of custom properties of a room. Set via Room.SetCustomProperties (not available for RoomInfo class!).</summary>
        /// <remarks>All keys are string-typed and the values depend on the game/application.</remarks>
        /// <see cref="Room.SetCustomProperties"/>
        public PhotonHashtable CustomProperties
        {
            get
            {
                return roomInfo.CustomProperties;
            }
        }

        /// <summary>The name of a room. Unique identifier for a room/match (per AppId + game-Version).</summary>
        public string Name
        {
            get
            {
                return roomInfo.Name;
            }
        }

        /// <summary>
        /// Count of players currently in room. This property is overwritten by the Room class (used when you're in a Room).
        /// </summary>
        public int PlayerCount { get => roomInfo.PlayerCount;}

        /// <summary>
        /// The limit of players for this room. This property is shown in lobby, too.
        /// If the room is full (players count == maxplayers), joining this room will fail.
        /// </summary>
        /// <remarks>
        /// As part of RoomInfo this can't be set.
        /// As part of a Room (which the player joined), the setter will update the server and all clients.
        /// </remarks>
        public int MaxPlayers
        {
            get
            {
                return roomInfo.MaxPlayers;
            }
        }

        /// <summary>
        /// Defines if the room can be joined.
        /// This does not affect listing in a lobby but joining the room will fail if not open.
        /// If not open, the room is excluded from random matchmaking.
        /// Due to racing conditions, found matches might become closed even while you join them.
        /// Simply re-connect to master and find another.
        /// Use property "IsVisible" to not list the room.
        /// </summary>
        /// <remarks>
        /// As part of RoomInfo this can't be set.
        /// As part of a Room (which the player joined), the setter will update the server and all clients.
        /// </remarks>
        public bool IsOpen
        {
            get
            {
                return roomInfo.IsOpen;
            }
        }

        /// <summary>
        /// Defines if the room is listed in its lobby.
        /// Rooms can be created invisible, or changed to invisible.
        /// To change if a room can be joined, use property: open.
        /// </summary>
        /// <remarks>
        /// As part of RoomInfo this can't be set.
        /// As part of a Room (which the player joined), the setter will update the server and all clients.
        /// </remarks>
        public bool IsVisible
        {
            get
            {
                return roomInfo.IsVisible;
            }
        }

        /// <summary>
        /// Constructs a RoomInfo to be used in room listings in lobby.
        /// </summary>
        /// <param name="roomName">Name of the room and unique ID at the same time.</param>
        /// <param name="roomProperties">Properties for this room.</param>
        protected internal LobbyRoomInfo(RoomInfo room)
        {
            roomInfo = room;
        }

        /// <summary>
        /// Makes RoomInfo comparable (by name).
        /// </summary>
        public override bool Equals(object other)
        {
            LobbyRoomInfo otherRoomInfo = other as LobbyRoomInfo;
            return (otherRoomInfo != null && this.Name.Equals(otherRoomInfo.Name));
        }

        /// <summary>
        /// Accompanies Equals, using the name's HashCode as return.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public string GetDebugLabel() // Works for Room too!
        {
            // Replacement for room.ToString()
            return roomInfo.GetDebugLabel();
        }

        /// <summary>Returns most interesting room values as string.</summary>
        /// <returns>Summary of this RoomInfo instance.</returns>
        public override string ToString()
        {
            return string.Format("Room: '{0}' {1},{2} {4}/{3} players.{5}", this.Name, this.IsVisible ? "visible" : "hidden", this.IsOpen ? "open" : "closed", this.MaxPlayers, this.PlayerCount, this.RemovedFromList ? " removed!" : "");
        }

        /// <summary>Returns most interesting room values as string, including custom properties.</summary>
        /// <returns>Summary of this RoomInfo instance.</returns>
        public string ToStringFull()
        {
            return string.Format("Room: '{0}' {1},{2} {4}/{3} players.\ncustomProps: {5}", this.Name, this.IsVisible ? "visible" : "hidden", this.IsOpen ? "open" : "closed", this.MaxPlayers, this.PlayerCount, this.CustomProperties.ToStringFull());
        }
    }
}
