using Photon.Pun;
using Photon.Realtime;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Some convenience methods to check Photon network state for connectivity.
    /// </summary>
    public static class PhotonWrapper
    {
        public static bool IsPhotonReady => PhotonNetwork.NetworkClientState == ClientState.PeerCreated ||
                                            PhotonNetwork.NetworkClientState == ClientState.Disconnected ||
                                            PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer;

        /// <summary>
        /// Can connect to Photon (master server) with our settings if we are just created or disconnected.
        /// </summary>
        /// <remarks>
        /// <c>NetworkClientState</c> is PeerCreated || Disconnected.
        /// </remarks>
        public static bool CanConnect => PhotonNetwork.NetworkClientState == ClientState.PeerCreated ||
                                         PhotonNetwork.NetworkClientState == ClientState.Disconnected;

        /// <summary>
        /// Can join lobby if we are connected to a master server.
        /// </summary>
        /// <remarks>
        /// <c>NetworkClientState</c> is ConnectedToMasterServer.
        /// </remarks>
        public static bool CanJoinLobby => PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer;

        public static bool InRoom => PhotonNetwork.InRoom;
        public static bool InLobby => PhotonNetwork.InLobby;
    }
}