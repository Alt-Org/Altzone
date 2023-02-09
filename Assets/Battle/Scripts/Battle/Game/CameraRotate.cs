using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Battle.Scripts.Battle.Game
{
    internal class CameraRotate : MonoBehaviour
    {
        [SerializeField] Transform Camera;
        [SerializeField] Transform Background;
        [SerializeField] Transform GridOverlay;
        private bool team2 = false;

        private void Start()
        {
            if (PhotonNetwork.InRoom)
            {
                var player = PhotonNetwork.LocalPlayer;
                var playerPos = PhotonBattle.GetPlayerPos(player);
                var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
                Debug.Log($"teaugygygggum {teamNumber} pos {playerPos} {player.GetDebugLabel()}");
                if (teamNumber == 2)   //2
                {
                    Camera.eulerAngles = new Vector3(0, 0, 180);
                    Background.eulerAngles = new Vector3(0, 0, 180);
                    GridOverlay.eulerAngles = new Vector3(0, 0, 180);
                    team2 = true;
                }
            }
        }

        private void Update()
        {
            if (team2 == true)
            {
                GridOverlay.eulerAngles = new Vector3(0, 0, 0);
            }
        }
    }
}