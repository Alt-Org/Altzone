using System.Collections;
using Altzone.Scripts.Config;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Lobby.Scripts.InLobby
{
    public class InLobbyController : MonoBehaviour
    {
        [SerializeField] private InLobbyView _view;

        private void Awake()
        {
            _view.CharacterButton.onClick.AddListener(CharacterButtonOnClick);
            _view.RoomButton.onClick.AddListener(RoomButtonOnClick);

        }
        
        private void OnEnable()
        {
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState}");
            _view.Reset();
            _view.TitleText = $"Welcome to {Application.productName} {PhotonLobby.GameVersion}";
            _view.LobbyText = string.Empty;

            StartCoroutine(StartLobby());
        }

        private IEnumerator StartLobby()
        {
            var delay = new WaitForSeconds(0.1f);
            while (!PhotonNetwork.InLobby)
            {
                Debug.Log($"{PhotonNetwork.NetworkClientState}");
                if (PhotonWrapper.InRoom)
                {
                    PhotonLobby.LeaveRoom();
                }
                else if (PhotonWrapper.CanConnect)
                {
                    var playerData = RuntimeGameConfig.Get().PlayerDataCache;
                    PhotonLobby.Connect(playerData.PlayerName);
                }
                else if (PhotonWrapper.CanJoinLobby)
                {
                    PhotonLobby.JoinLobby();
                }
                yield return delay;
            }
            _view.EnableButtons();
        }

        private void Update()
        {
            if (!PhotonNetwork.InLobby)
            {
                return;
            }
            var playerCount = PhotonNetwork.CountOfPlayers;
            _view.LobbyText = playerCount == 1
                ? "You are the only player here"
                : $"There is {playerCount} players online";
            ;
        }

        private void CharacterButtonOnClick()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
        }

        private void RoomButtonOnClick()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
        }
    }
}