using Photon.Pun;
using Photon.Realtime;

namespace Prg.Scripts.Common.Photon
{
    public static class PhotonWrapper
    {
        public static string NetworkClientState => PhotonNetwork.NetworkClientState.ToString();

        public static bool InRoom => PhotonNetwork.InRoom;

        public static bool InLobby => PhotonNetwork.InLobby;

        public static bool IsMasterClient => PhotonNetwork.IsMasterClient;

        /// <summary>
        /// Can join lobby if we are connected to a master server.
        /// </summary>
        /// <remarks>
        /// <c>NetworkClientState</c> is ConnectedToMasterServer.
        /// </remarks>
        public static bool CanJoinLobby => PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer;

        /// <summary>
        /// Can connect to Photon backend system with our settings if we are just created or disconnected.
        /// </summary>
        /// <remarks>
        /// <c>NetworkClientState</c> is PeerCreated || Disconnected.
        /// </remarks>
        public static bool CanConnect => PhotonNetwork.NetworkClientState == ClientState.PeerCreated ||
                                         PhotonNetwork.NetworkClientState == ClientState.Disconnected;

        /// <summary>
        /// Photon network is idle or on master server.
        /// </summary>
        /// <remarks>
        /// <c>NetworkClientState</c> is PeerCreated || Disconnected || ConnectedToMasterServer.
        /// </remarks>
        public static bool IsPhotonReady => isPhotonReady();

        public static void LoadLevel(string levelUnityName)
        {
            PhotonNetwork.LoadLevel(levelUnityName);
        }

        private static bool isPhotonReady()
        {
            var state = PhotonNetwork.NetworkClientState;
            return state == ClientState.PeerCreated || state == ClientState.Disconnected || state == ClientState.ConnectedToMasterServer;
        }
    }
}