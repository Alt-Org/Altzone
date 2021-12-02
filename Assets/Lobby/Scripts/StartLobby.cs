using Altzone.Scripts.Config;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Lobby.Scripts
{
    /// <summary>
    /// Helper class to enter Photon lobby.
    /// </summary>
    public class StartLobby : MonoBehaviour
    {
        [SerializeField] private GameObject inLobby;
        [SerializeField] private GameObject inRoom;
        [SerializeField] private GameObject inChooseModel;

        private static StartLobby instance;
        private void Start()
        {
            instance = this;
            PhotonLobby.OfflineMode = false;
            inLobby.SetActive(false);
            inRoom.SetActive(false);
            inChooseModel.SetActive(false);
        }

        private void Update()
        {
            if (PhotonWrapper.InLobby)
            {
                var playerData = RuntimeGameConfig.Get().PlayerDataCache;
                if (PhotonNetwork.NickName != playerData.PlayerName)
                {
                    // Fix player name if it has been changed
                    PhotonNetwork.NickName = playerData.PlayerName;
                }
                inLobby.SetActive(true);
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

        public void showLobby()
        {
            Debug.Log("showLobby");
            inLobby.SetActive(true);
            inRoom.SetActive(false);
            inChooseModel.SetActive(false);
        }

        public static void showChooseModel()
        {
            instance.inLobby.SetActive(false);
            instance.inRoom.SetActive(false);
            instance.inChooseModel.SetActive(true);
        }
    }
}
