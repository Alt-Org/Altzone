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
        public List<GameObject> PlayerActors = new List<GameObject>();
        public PlayerDriverPhoton PlayerDriverPhoton;
        [SerializeField] PlayerActor PlayerActor;
        public PhotonView View;

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

            PlayerActors = GameObject.FindGameObjectsWithTag("PlayerDriverPhoton").ToList();
            foreach (GameObject t in PlayerActors)
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
            }
            else
            {
                TeamDiamonds = GameObject.FindGameObjectWithTag("BetaDiamonds");
            }
            DiamondText = TeamDiamonds.GetComponent<TMP_Text>();
            TeamDiamondCount = TeamDiamonds.GetComponent<TeamDiamondCount>();
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
                    object[] content = new object[] { EventSender };
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
                if (EventSender == PlayerActor.SeePlayerName)
                {
                    TeamDiamondCount.TeamDiamondCounter = TeamDiamondCount.TeamDiamondCounter + 1;
                    DiamondText.SetText(TeamDiamondCount.TeamDiamondCounter.ToString());
                    //collectionSoundEffect.PlayOneShot(collect);
                }
            }
        }
    }
}
