using System.Collections;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Lobby.Scripts.InLobby
{
    public class InLobbyController : MonoBehaviour
    {
        [SerializeField] private InLobbyView _view;
        [SerializeField] private StartLobby _startLobby;

        private void Awake()
        {
            Debug.Log("Awake");
            _view.TitleText = $"Welcome to {Application.productName} {PhotonLobby.gameVersion}";
            _view.LobbyText = string.Empty;

            _view.CharacterButton.onClick.AddListener(CharacterButton);
            _view.RoomButton.onClick.AddListener(RoomButton);
        }

        private void OnEnable()
        {
            _startLobby.enabled = true;
            _view.RoomButton.interactable = false;
            StartCoroutine(WaitForLobby());
        }

        private IEnumerator WaitForLobby()
        {
            yield return new WaitUntil(() => PhotonNetwork.InLobby);
            _view.RoomButton.interactable = true;
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

        private void CharacterButton()
        {
            Debug.Log("CharacterButton");
        }

        private void RoomButton()
        {
            Debug.Log("RoomButton");
        }
    }
}