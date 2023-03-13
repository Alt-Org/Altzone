using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace Battle.Scripts.Battle.Game
{
    internal class Pickup : MonoBehaviour
    {
        //[SerializeField] AudioClip collect;
        //[SerializeField] private AudioSource collectionSoundEffect;
        //public int LocalDiamondCount = 0;
        public GameObject TeamDiamonds;
        //public TMP_Text TeamDiamonds;

        private TMP_Text DiamondText;
        public TeamDiamondCount TeamDiamondCount;

        //public PhotonView View;

        private void Start()
        {
            //View = GetComponent<PhotonView>();
            //if (View.IsMine == true)
            //{
                var player = PhotonNetwork.LocalPlayer;
                var playerPos = PhotonBattle.GetPlayerPos(player);
                var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
                Debug.Log($"Pickupteam {teamNumber} pos {playerPos} {player.GetDebugLabel()}");
                if (teamNumber == 1)
                {
                    TeamDiamonds = GameObject.FindGameObjectWithTag("AlphaDiamonds");
                }
                else
                {
                    TeamDiamonds = GameObject.FindGameObjectWithTag("BetaDiamonds");
                }
                DiamondText = TeamDiamonds.GetComponent<TMP_Text>();     ///TeamDiamonds.TMP_Text;
                TeamDiamondCount = TeamDiamonds.GetComponent<TeamDiamondCount>();
            //}
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Diamond"))
            {
                //if (View.IsMine == true)
                //{
                    TeamDiamondCount.TeamDiamondCounter = TeamDiamondCount.TeamDiamondCounter + 1;
                    //TeamDiamonds = DiamondCount;
                    DiamondText.SetText(TeamDiamondCount.TeamDiamondCounter.ToString());
                    //collectionSoundEffect.PlayOneShot(collect);
                //}
                Destroy(collision.gameObject);
            }
        }
    }
}