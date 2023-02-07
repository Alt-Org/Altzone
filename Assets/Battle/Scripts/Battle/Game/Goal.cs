using System.Collections;
using System.Collections.Generic;
using UnityConstants;
using UnityEngine;
using Photon.Pun;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Removes a brick from the wall when hit conditions are met.
    /// </summary>
    internal class Goal : MonoBehaviour
    {
        [SerializeField] GameObject WinText;
        [SerializeField] GameObject LoseText;
        [SerializeField] GameObject LobbyButton;
        [SerializeField] BoxCollider2D _WallCollider;
        [SerializeField] int TestLimit;
        [SerializeField] int GoalNumber;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))      // && PhotonNetwork.IsMasterClient
            {
                //string tempString = "gahgsahsgfahgsh";
	            //photonView.RPC("SetAll", PhotonTargets.All, tempString);
                if (PhotonNetwork.InRoom)
                {
                    var player = PhotonNetwork.LocalPlayer;
                    var playerPos = PhotonBattle.GetPlayerPos(player);
                    var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
                    Debug.Log($"team {teamNumber} pos {playerPos} {player.GetDebugLabel()}");

                    if (GoalNumber != teamNumber)
                    {
                        WinText.SetActive(true);
                    }
                    else
                    {
                        LoseText.SetActive(true);
                    }
                    LobbyButton.SetActive(true);
                }   
            }
        }

        /*[PunRPC]
        void SetAll (string tempString, int number) 
        {
            Debug.Log(tempString + " " + number);
        }*/
        
        private void Update()
        {
            if(GameObject.FindGameObjectsWithTag("PlayerDriverPhoton").Length > TestLimit) {
                _WallCollider.isTrigger = true;
            }
        }
    }
}