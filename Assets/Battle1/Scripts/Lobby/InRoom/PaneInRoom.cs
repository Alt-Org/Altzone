using Battle1.Scripts.Lobby;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;
using PhotonBattle = Altzone.Scripts.Battle.Photon.PhotonBattleRoom;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Battle1.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Middle pane in lobby while in room to manage current player "state".
    /// </summary>
    public class PaneInRoom : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text _battleID;
        [SerializeField] private Button[] buttons;

        private void Start()
        {
            buttons[0].onClick.AddListener(SetPlayerAsGuest);
            buttons[1].onClick.AddListener(SetPlayerAsSpectator);
            buttons[2].onClick.AddListener(StartPlaying);
            buttons[3].onClick.AddListener(StartRaidTest);
        }

        private void SetPlayerAsGuest()
        {
            Debug.Log($"setPlayerAsGuest {PhotonBattle.PlayerPositionGuest}");
           /* this.Publish(new LobbyManagerOld.PlayerPosEvent(PhotonBattle.PlayerPositionGuest));*/
        }

        private void SetPlayerAsSpectator()
        {
            Debug.Log($"setPlayerAsSpectator {PhotonBattle.PlayerPositionSpectator}");
            /*this.Publish(new LobbyManagerOld.PlayerPosEvent(PhotonBattle.PlayerPositionSpectator));*/
        }

        private void StartPlaying()
        {
            Debug.Log($"startPlaying");
           /* this.Publish(new LobbyManagerOld.StartPlayingEvent());*/
        }

        private void StartRaidTest()
        {
            Debug.Log($"startPlaying");
           /* this.Publish(new LobbyManagerOld.StartRaidTestEvent());*/
        }

        /// <summary>
        /// Stupid way to poll network state changes on every frame!
        /// </summary>
        private void Update()
        {
            /*_battleID.text = PhotonNetwork.InRoom ? $"({PhotonNetwork.CurrentRoom.GetCustomProperty<string>("bid")})" : "<color=red>Not in room</color>";
            title.text = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "<color=red>Not in room</color>";*/
        }
    }
}
