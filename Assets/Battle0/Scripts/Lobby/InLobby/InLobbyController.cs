using System.Collections;
using Altzone.Scripts.Config;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Battle0.Scripts.Lobby.InLobby
{
    public class InLobbyController : MonoBehaviour
    {
        [SerializeField] private InLobbyView _view;

        private void Awake()
        {
            _view.CharacterButtonOnClick = CharacterButtonOnClick;
            _view.RoomButtonOnClick = RoomButtonOnClick;
            _view.QuickGameButtonOnClick = QuickGameButtonOnClick;
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState}");
            _view.Reset();
            UpdateTitle();
            _view.LobbyText = string.Empty;
            StartCoroutine(StartLobby());
        }

        private void UpdateTitle()
        {
            _view.TitleText = $"{Application.productName} {PhotonLobby.GameVersion} {PhotonLobby.GetRegion()}";
        }
        private IEnumerator StartLobby()
        {
            var delay = new WaitForSeconds(0.1f);
            while (!PhotonNetwork.InLobby)
            {
                Debug.Log($"{PhotonNetwork.NetworkClientState}");
                if (PhotonNetwork.InRoom)
                {
                    PhotonLobby.LeaveRoom();
                }
                else if (PhotonWrapper.CanConnect)
                {
                    var playerData = GameConfig.Get().PlayerSettings;
                    PhotonLobby.Connect(playerData.PlayerName);
                }
                else if (PhotonWrapper.CanJoinLobby)
                {
                    PhotonLobby.JoinLobby();
                }
                yield return delay;
            }
            UpdateTitle();
            _view.EnableButtons();
        }

        private void Update()
        {
            if (!PhotonNetwork.InLobby)
            {
                _view.LobbyText = "Wait";
                return;
            }
            var playerCount = PhotonNetwork.CountOfPlayers;
            string text;
            switch (playerCount)
            {
                case 0:
                    text = "Wait";
                    break;
                case 1:
                    text = "You are the only player";
                    break;
                default:
                    text = $"There are {playerCount} players";
                    break;
            }
            _view.LobbyText = $"{text}, ping {PhotonNetwork.GetPing()}";
        }

        private void CharacterButtonOnClick()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
        }

        private void RoomButtonOnClick()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
        }

        private void QuickGameButtonOnClick()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
        }
    }
}