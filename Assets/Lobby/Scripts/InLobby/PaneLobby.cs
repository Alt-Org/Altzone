using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InLobby
{
    public class PaneLobby : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text lobbyText;
        [SerializeField] private Button characterButton;

        private void Start()
        {
            titleText.text = $"Welcome to {Application.productName} {PhotonLobby.gameVersion}";
            characterButton.onClick.AddListener(StartLobby.showChooseModel);
        }

        private void Update()
        {
            if (!PhotonNetwork.InLobby)
            {
                return;
            }
            var playerCount = PhotonNetwork.CountOfPlayers;
            lobbyText.text = playerCount == 1
                ? "You are the only player here"
                : $"There is {playerCount} players online";
            ;
        }
    }
}