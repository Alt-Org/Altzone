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
        [SerializeField] GameObject WinLoseText;
        [SerializeField] GameObject LobbyButton;
        [SerializeField] BoxCollider2D _bottomWallCollider;
        [SerializeField] BoxCollider2D _topWallCollider;
        [SerializeField] int TestLimit;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))      // && PhotonNetwork.IsMasterClient
            {
                //string tempString = "gahgsahsgfahgsh";
	            //photonView.RPC("SetAll", PhotonTargets.All, tempString);
                WinLoseText.SetActive(true);
                LobbyButton.SetActive(true);   
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
                _bottomWallCollider.isTrigger = true;
                _topWallCollider.isTrigger = true;
            }
            /*if (PlayerActor > 1)
            {
                _isBackWallsTriggers = true;
            }*/
        }
    }
}