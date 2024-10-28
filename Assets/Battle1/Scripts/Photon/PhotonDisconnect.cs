using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    public class PhotonDisconnect : MonoBehaviour
    {
        private void Update()
        {
            if (PhotonWrapper.InRoom)
            {
                PhotonLobby.LeaveRoom();
                return;
            }
            if (PhotonWrapper.InLobby)
            {
                PhotonLobby.Disconnect();
                return;
            }
            if (PhotonWrapper.CanJoinLobby)
            {
                PhotonLobby.Disconnect();
                return;
            }
            if (PhotonWrapper.CanConnect)
            {
                enabled = false;
            }
        }
    }
}