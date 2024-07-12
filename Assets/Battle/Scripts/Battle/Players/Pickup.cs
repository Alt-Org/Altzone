using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Battle.Scripts.Battle.Players
{
    internal class Pickup : MonoBehaviour, IOnEventCallback
    {
        //[SerializeField] AudioClip collect;
        //[SerializeField] private AudioSource collectionSoundEffect;
        public GameObject TeamDiamonds;
        private TMP_Text DiamondText;
        public TeamDiamondCount TeamDiamondCount;
        public GameObject TeamDiamonds2;
        private TMP_Text DiamondText2;
        public TeamDiamondCount TeamDiamondCount2;
        public GameObject TeamDiamonds3;
        private TMP_Text DiamondText3;
        public TeamDiamondCount TeamDiamondCount3;
        public GameObject TeamDiamonds4;
        private TMP_Text DiamondText4;
        public TeamDiamondCount TeamDiamondCount4;

        public List<GameObject> PlayerDriverPhotons = new List<GameObject>();
        public PlayerDriverPhoton PlayerDriverPhoton;
        [SerializeField] PlayerActor PlayerActor;
        public PhotonView View;

        public int OwnDiamonds1;
        public int TeamNumber;

        private const byte PickupEvent = 0;
        public string EventSender;

        private void Start()
        {
            if (!PhotonNetwork.InRoom)
            {
                enabled = false;
                return;
            }

            PlayerDriverPhotons = GameObject.FindGameObjectsWithTag("PlayerDriverPhoton").ToList();
            foreach (GameObject t in PlayerDriverPhotons)
            {
                View = t.GetComponent<PhotonView>();
                PlayerDriverPhoton = t.GetComponent<PlayerDriverPhoton>();
                if (PlayerActor.SeePlayerName == PlayerDriverPhoton.PlayerName)
                {
                    break;
                }
            }

            var player = View.Owner;
            var playerPos = PhotonBattle.GetPlayerPos(player);
            TeamNumber = PhotonBattle.GetTeamNumber(playerPos);
            if (TeamNumber == 1)
            {
                TeamDiamonds = GameObject.FindGameObjectWithTag("AlphaDiamonds");
                TeamDiamonds2 = GameObject.FindGameObjectWithTag("AlphaDiamonds2");
                TeamDiamonds3 = GameObject.FindGameObjectWithTag("AlphaDiamonds3");
                TeamDiamonds4 = GameObject.FindGameObjectWithTag("AlphaDiamonds4");
            }
            else
            {
                TeamDiamonds = GameObject.FindGameObjectWithTag("BetaDiamonds");
                TeamDiamonds2 = GameObject.FindGameObjectWithTag("BetaDiamonds2");
                TeamDiamonds3 = GameObject.FindGameObjectWithTag("BetaDiamonds3");
                TeamDiamonds4 = GameObject.FindGameObjectWithTag("BetaDiamonds4");
            }
            DiamondText = TeamDiamonds.GetComponent<TMP_Text>();
            TeamDiamondCount = TeamDiamonds.GetComponent<TeamDiamondCount>();
            DiamondText2 = TeamDiamonds2.GetComponent<TMP_Text>();
            TeamDiamondCount2 = TeamDiamonds2.GetComponent<TeamDiamondCount>();
            DiamondText3 = TeamDiamonds3.GetComponent<TMP_Text>();
            TeamDiamondCount3 = TeamDiamonds3.GetComponent<TeamDiamondCount>();
            DiamondText4 = TeamDiamonds4.GetComponent<TMP_Text>();
            TeamDiamondCount4 = TeamDiamonds4.GetComponent<TeamDiamondCount>();
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Diamond"))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    EventSender = PlayerActor.SeePlayerName;
                    var DiamondType = 1;
                    object[] content = new object[] { EventSender, DiamondType };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(PickupEvent, content, raiseEventOptions, SendOptions.SendReliable);
                }
                Destroy(collision.gameObject);
            }
            if (collision.gameObject.CompareTag("Diamond2"))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    EventSender = PlayerActor.SeePlayerName;
                    var DiamondType = 2;
                    object[] content = new object[] { EventSender, DiamondType };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(PickupEvent, content, raiseEventOptions, SendOptions.SendReliable);
                }
                Destroy(collision.gameObject);
            }
            if (collision.gameObject.CompareTag("Diamond3"))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    EventSender = PlayerActor.SeePlayerName;
                    var DiamondType = 3;
                    object[] content = new object[] { EventSender, DiamondType };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(PickupEvent, content, raiseEventOptions, SendOptions.SendReliable);
                }
                Destroy(collision.gameObject);
            }
            if (collision.gameObject.CompareTag("Diamond4"))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    EventSender = PlayerActor.SeePlayerName;
                    var DiamondType = 4;
                    object[] content = new object[] { EventSender, DiamondType };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(PickupEvent, content, raiseEventOptions, SendOptions.SendReliable);
                }
                Destroy(collision.gameObject);
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            if (eventCode == PickupEvent)
            {
                object[] content = (object[])photonEvent.CustomData;
                string EventSender = (string)content[0];
                int DiamondType = (int)content[1];
                if (EventSender == PlayerActor.SeePlayerName && DiamondType == 1)
                {
                    OwnDiamonds1 = OwnDiamonds1 + 1;
                    TeamDiamondCount.TeamDiamondCounter = TeamDiamondCount.TeamDiamondCounter + 1;
                    DiamondText.SetText(TeamDiamondCount.TeamDiamondCounter.ToString());
                    //collectionSoundEffect.PlayOneShot(collect);
                }
                if (EventSender == PlayerActor.SeePlayerName && DiamondType == 2)
                {
                    TeamDiamondCount2.TeamDiamondCounter = TeamDiamondCount2.TeamDiamondCounter + 1;
                    DiamondText2.SetText(TeamDiamondCount2.TeamDiamondCounter.ToString());
                    //collectionSoundEffect.PlayOneShot(collect);
                }
                if (EventSender == PlayerActor.SeePlayerName && DiamondType == 3)
                {
                    TeamDiamondCount3.TeamDiamondCounter = TeamDiamondCount3.TeamDiamondCounter + 1;
                    DiamondText3.SetText(TeamDiamondCount3.TeamDiamondCounter.ToString());
                    //collectionSoundEffect.PlayOneShot(collect);
                }
                if (EventSender == PlayerActor.SeePlayerName && DiamondType == 4)
                {
                    TeamDiamondCount4.TeamDiamondCounter = TeamDiamondCount4.TeamDiamondCounter + 1;
                    DiamondText4.SetText(TeamDiamondCount4.TeamDiamondCounter.ToString());
                    //collectionSoundEffect.PlayOneShot(collect);
                }
            }
        }
    }
}
