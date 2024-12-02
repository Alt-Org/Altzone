using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Altzone.Scripts.Lobby.Wrappers
{
    public class LobbyWrapper
    {
        /// <summary>
        /// Info for a lobby on the server. Used when <see cref="AppSettings.EnableLobbyStatistics"/> is true (can be passed to ConnectUsingSettings).
        /// </summary>
        public class TypedLobbyInfoWrapper
        {
            private TypedLobbyInfo _lobbyInfo;
            /// <summary>Count of players that currently joined this lobby.</summary>
            public int PlayerCount => _lobbyInfo.PlayerCount;

            /// <summary>Count of rooms currently associated with this lobby.</summary>
            public int RoomCount => _lobbyInfo.RoomCount;

            internal TypedLobbyInfoWrapper(TypedLobbyInfo lobbyinfo)
            {
                _lobbyInfo = lobbyinfo;
            }

            /// <summary>Returns a string representation of this TypedLobbyInfo.</summary>
            /// <returns>String representation of this TypedLobbyInfo.</returns>
            public override string ToString()
            {
                return _lobbyInfo.ToString();
            }
        }


        /// <summary>Refers to a specific lobby on the server.</summary>
        /// <remarks>
        /// Name and Type combined are the unique identifier for a lobby.<br/>
        /// The server will create lobbies "on demand", so no registration or setup is required.<br/>
        /// An empty or null Name always points to the "default lobby" as special case.
        /// </remarks>
        public class TypedLobbyWrapper
        {
            private TypedLobby _lobby;
            /// <summary>
            /// Name of the lobby. Default: null, pointing to the "default lobby".
            /// </summary>
            /// <remarks>
            /// If Name is null or empty, a TypedLobby will point to the "default lobby". This ignores the Type value and always acts as  <see cref="LobbyType.Default"/>.
            /// </remarks>
            public string Name => _lobby.Name;

            /// <summary>
            /// Type (and behaviour) of the lobby.
            /// </summary>
            /// <remarks>
            /// If Name is null or empty, a TypedLobby will point to the "default lobby". This ignores the Type value and always acts as  <see cref="LobbyType.Default"/>.
            /// </remarks>
            public LobbyType Type => _lobby.Type;


            /// <summary>
            /// A reference to the default lobby which is the unique lobby that uses null as name and is of type <see cref="LobbyType.Default"/>.
            /// </summary>
            /// <remarks>
            /// There is only a single lobby with an empty name on the server. It is always of type <see cref="LobbyType.Default"/>.<br/>
            /// On the other hand, this is a shortcut and reusable reference to the default lobby.<br/>
            /// </remarks>
            public static TypedLobbyWrapper Default
            {
                get
                {
                    return new(TypedLobby.Default);
                }
            }

            /// <summary>
            /// Returns if this instance points to the "default lobby" (<see cref="TypedLobby.Default"/>).
            /// </summary>
            /// <remarks>
            /// This comes up to checking if the Name is null or empty.
            /// <see cref="LobbyType.Default"/> is not the same thing as the "default lobby" (<see cref="TypedLobby.Default"/>).
            /// </remarks>
            public bool IsDefault
            {
                get { return _lobby.IsDefault; }
            }


            /// <summary>
            /// Creates a new TypedLobby instance, initialized to the given values.
            /// </summary>
            /// <param name="name">Some string to identify a lobby. Should be non null and non empty.</param>
            /// <param name="type">The type of a lobby defines its behaviour.</param>
            public TypedLobbyWrapper(string name, LobbyLobbyType type)
            {
                _lobby = new TypedLobby(name, (LobbyType)type);
            }

            /// <summary>
            /// Creates a new TypedLobby instance, initialized to the values of the given original.
            /// </summary>
            /// <param name="original">Used to initialize the new instance.</param>
            public TypedLobbyWrapper(TypedLobby original = null)
            {
                if (original != null)
                    _lobby = original;
                else
                    _lobby = TypedLobby.Default;
            }

            /// <summary>Returns a string representation of this TypedLobby.</summary>
            /// <returns>String representation of this TypedLobby.</returns>
            public override string ToString()
            {
                return $"'{this.Name}'[{this.Type}]";
            }

            public TypedLobby GetOriginal() { return _lobby; }
        }
    }
}
