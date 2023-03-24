using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class CameraRotate : MonoBehaviour
    {
        [SerializeField] Transform Camera;
        [SerializeField] Transform Background;
        [SerializeField] Transform GridOverlay;
        [SerializeField] Transform DiamondCounters;
        [SerializeField] Transform BetaDiamonds;
        [SerializeField] Transform AlphaDiamonds;
        //private bool team2 = false;

        private void Start()
        {
            if (PhotonNetwork.InRoom)
            {
                var player = PhotonNetwork.LocalPlayer;
                var playerPos = PhotonBattle.GetPlayerPos(player);
                var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
                Debug.Log($"teamNumber {teamNumber} pos {playerPos} {player.GetDebugLabel()}");
                if (teamNumber == 2)   //2
                {
                    Camera.eulerAngles = new Vector3(0, 0, 180);
                    Background.eulerAngles = new Vector3(0, 0, 180);
                    GridOverlay.eulerAngles = new Vector3(0, 0, 180);
                    DiamondCounters.eulerAngles = new Vector3(0, 0, 180);
                    BetaDiamonds.eulerAngles = new Vector3(0, 0, 0);
                    AlphaDiamonds.eulerAngles = new Vector3(0, 0, 0);
                    //team2 = true;
                }
            }
        }
    }
}