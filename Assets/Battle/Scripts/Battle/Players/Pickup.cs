using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
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

        public int TeamNumber;

        private void Start()
        {
            if (!PhotonNetwork.InRoom)
            {
                enabled = false;
                return;
            }
            //PlayerActors.Add(GetAllPlayerDrivers);
            PlayerActors = GameObject.FindGameObjectsWithTag("PlayerDriverPhoton").ToList();     //GetAllPlayerDrivers

            //while (RightDriver == false)
            //{
                foreach (GameObject t in PlayerActors)      //IPlayerDriver t in
                {
                    View = t.GetComponent<PhotonView>();
                    PlayerDriverPhoton = t.GetComponent<PlayerDriverPhoton>();
                    if (PlayerActor.SeePlayerName == PlayerDriverPhoton.PlayerName)       //PlayerActor.SeePlayerName
                    {
                        RightDriver = true;
                        Debug.Log($"{PlayerActor.SeePlayerName}ggrgtgrfefhbtrfsfvrfhbrgefe");       //PlayerActor.SeePlayerName
                        break;
                    }
                    Debug.Log($"players {GameObject.FindGameObjectsWithTag("PlayerDriverPhoton").Length}");      //GetAllPlayerDrivers.Count
                }

            //}
            Debug.Log($"{RightDriver}");

            /*if (local == true)
            {*/
                //Player Get (int id);
                var player = View.Owner;         //PhotonNetwork.LocalPlayer
                var playerPos = PhotonBattle.GetPlayerPos(player);
                TeamNumber = PhotonBattle.GetTeamNumber(playerPos);
                Debug.Log($"Pickupteam {TeamNumber} pos {playerPos} {player.GetDebugLabel()}");
                if (TeamNumber == 1)
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
