using Altzone.Scripts.Battle;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InRoom
{
    /// <summary>
    /// Middle pane in lobby while in room to manage current player "state".
    /// </summary>
    public class PaneInRoom : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Button[] buttons;

        private void Start()
        {
            buttons[0].onClick.AddListener(SetPlayerAsGuest);
            buttons[1].onClick.AddListener(SetPlayerAsSpectator);
            buttons[2].onClick.AddListener(StartPlaying);
        }

        private void SetPlayerAsGuest()
        {
            Debug.Log($"setPlayerAsGuest {PhotonBattle.PlayerPositionGuest}");
            this.Publish(new LobbyManager.PlayerPosEvent(PhotonBattle.PlayerPositionGuest));
        }

        private void SetPlayerAsSpectator()
        {
            Debug.Log($"setPlayerAsSpectator {PhotonBattle.PlayerPositionSpectator}");
            this.Publish(new LobbyManager.PlayerPosEvent(PhotonBattle.PlayerPositionSpectator));
        }

        private void StartPlaying()
        {
            Debug.Log($"startPlaying {PhotonBattle.StartPlayingEvent}");
            this.Publish(new LobbyManager.PlayerPosEvent(PhotonBattle.StartPlayingEvent));
        }

        /// <summary>
        /// Stupid way to poll network state changes on every frame!
        /// </summary>
        private void Update()
        {
            title.text = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "<color=red>Not in room</color>";
        }
    }
}