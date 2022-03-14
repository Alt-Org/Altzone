using Altzone.Scripts.Config;
using Photon.Pun;
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
            PhotonNetwork.OfflineMode = false;
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
                PhotonLobby.LeaveRoom();
                return;
            }
            if (PhotonWrapper.CanJoinLobby)
            {
                PhotonLobby.JoinLobby();
                return;
            }
            if (PhotonWrapper.CanConnect)
            {
                var playerData = RuntimeGameConfig.Get().PlayerDataCache;
                PhotonLobby.Connect(playerData.PlayerName);
            }
        }
    }
}
