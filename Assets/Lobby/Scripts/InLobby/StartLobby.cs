using Altzone.Scripts.Config;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Lobby.Scripts.InLobby
{
    /// <summary>
    /// Helper class to enter Photon lobby.
    /// </summary>
    public class StartLobby : MonoBehaviour
    {
        private void OnEnable()
        {
            PhotonLobby.OfflineMode = false;
        }

        private void Update()
        {
            if (PhotonWrapper.InLobby)
            {
                enabled = false;
                return;
            }
            if (PhotonWrapper.InRoom)
            {
                PhotonLobby.leaveRoom();
                return;
            }
            if (PhotonWrapper.CanJoinLobby)
            {
                PhotonLobby.joinLobby();
                return;
            }
            if (PhotonWrapper.CanConnect)
            {
                var playerData = RuntimeGameConfig.Get().PlayerDataCache;
                PhotonLobby.connect(playerData.PlayerName);
            }
        }
    }
}
