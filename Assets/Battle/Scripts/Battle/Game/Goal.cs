using System.Collections;
using System.Collections.Generic;
using UnityConstants;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using NUnit.Framework.Internal;
using TMPro;

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
        [SerializeField] GameObject RaidButton;
        [SerializeField] BoxCollider2D _WallCollider;
        [SerializeField] int TestLimit;
        [SerializeField] int GoalNumber;
        [SerializeField] TMP_Text CountDownText;
        [SerializeField] AudioSource _AudioSource;
        [SerializeField] AudioClip[] _AudioClips;

        [SerializeField, Header("End Screen Graphics")] private GameObject WinGraphics;
        [SerializeField] private GameObject LossGraphics;

        PlayerRole currentRole = PlayerRole.Player;
        private float timeLeft = 5.5f;
        private bool countingdown = false;
        

        public enum PlayerRole
        {
            Player,
            Spectator
        }
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
                countingdown = true;

                if (GoalNumber != teamNumber)
                {
                    //WinText.SetActive(true);
                    WinGraphics.SetActive(true);
                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                        { {"Role", (int)PlayerRole.Player } });
                        RaidButton.SetActive(true);
                    }
                }
                else
                {
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                    { {"Role", (int)PlayerRole.Spectator } });
                    //PhotonNetwork.LeaveRoom();
                    LossGraphics.SetActive(true);
                    //LobbyButton.SetActive(true);
                }
            }
        }
        private void Update()
        {
            if (countingdown)
            {
                if(timeLeft > 0)
                {
                    timeLeft -= Time.deltaTime;
                    CountDownText.text = timeLeft.ToString("F0");
                }
                else
                {
                    countingdown = false;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.LoadLevel("40-Raid");
                    }
                }
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
