using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Battle.Scripts.Battle.Players;

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
        //public List<IPlayerDriver>PlayerActors;     //<IPlayerDriver>();
        public List<GameObject> PlayerActors = new List<GameObject>();      //<IPlayerDriver>
        public PlayerDriverPhoton PlayerDriverPhoton;
        [SerializeField] PlayerActor PlayerActor;
        private bool RightDriver = false;
        public PhotonView View;

        private void Start()
        {
            if (!PhotonNetwork.InRoom)
            {
                enabled = false;
                return;
            }
            //PlayerActors.Add(GetAllPlayerDrivers);
            PlayerActors = Context.GetAllPlayerDriverObjects;     //GetAllPlayerDrivers

            while (RightDriver == false)
            {
                foreach (GameObject t in PlayerActors)      //IPlayerDriver t in
                {
                    View = t.GetComponent<PhotonView>();
                    PlayerDriverPhoton = t.GetComponent<PlayerDriverPhoton>();
                    if (PlayerActor.PlayerName == PlayerDriverPhoton.PlayerName)       //View.IsMine &&
                    {
                        RightDriver = true;
                        break;
                    }
                    Debug.Log($"players {Context.GetAllPlayerDriverObjects.Count}");      //GetAllPlayerDrivers.Count
                }

            }

            /*if (local == true)
            {*/
                //Player Get (int id);
                var player = View.Owner;         //PhotonNetwork.LocalPlayer
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
                /*if (local == true)
                {*/
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
