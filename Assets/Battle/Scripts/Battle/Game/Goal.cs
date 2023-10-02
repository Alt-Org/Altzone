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
    internal class Goal : MonoBehaviourPunCallbacks
    {
        [SerializeField] GameObject WinText;
        [SerializeField] GameObject LoseText;
        [SerializeField] GameObject LobbyButton;
        [SerializeField] GameObject RaidButton;
        [SerializeField] BoxCollider2D _WallCollider;
        [SerializeField] int TestLimit;
        [SerializeField] int GoalNumber;

        private void Start()
        {
            if (PhotonNetwork.CurrentRoom.Players.Count > TestLimit)
            {
                _WallCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball) && PhotonNetwork.CurrentRoom.Players.Count > TestLimit && PhotonNetwork.IsMasterClient)      // && PhotonNetwork.IsMasterClient
            {
                transform.GetComponent<PhotonView>().RPC("GoalRPC",  RpcTarget.All);
            }
        }
        
        [PunRPC]
        private void GoalRPC()
        {
            if (PhotonNetwork.InRoom)
                {
                    //_WallCollider.isTrigger = true;
                    var player = PhotonNetwork.LocalPlayer;
                    var playerPos = PhotonBattle.GetPlayerPos(player);
                    var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
                    Debug.Log($"team {teamNumber} pos {playerPos} {player.GetDebugLabel()}");

                    if (GoalNumber != teamNumber)
                    {
                        WinText.SetActive(true);
                        if (PhotonNetwork.IsMasterClient)
                        {
                            RaidButton.SetActive(true);
                        }
                    }
                    else
                    {
                        PhotonNetwork.LeaveRoom();
                        LoseText.SetActive(true);
                    }
                    LobbyButton.SetActive(true);
                }  
        }
        /*private void Update()
        {
            if(GameObject.FindGameObjectsWithTag("PlayerDriverPhoton").Length > TestLimit) {
                _WallCollider.isTrigger = true;
            }
        }*/
    }
}
