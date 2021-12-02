using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Lobby.Scripts.InRoom
{
    /// <summary>
    /// Helper class to switch UI to room view from lobby view.
    /// </summary>
    public class StartRoom: MonoBehaviour
    {
        private void Update()
        {
            if (PhotonWrapper.InRoom)
            {
                var room = PhotonNetwork.CurrentRoom;
                var player = PhotonNetwork.LocalPlayer;
                if (!room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "", out var uniquePlayerName))
                {
                    // Make player name unique within this room if it was not!
                    PhotonNetwork.NickName = uniquePlayerName;
                }
                enabled = false;
            }
        }
    }
}