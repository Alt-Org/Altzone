using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Battle.Scripts.Battle.Players;

namespace Battle.Scripts.Battle.Game
{
    internal class PickupDiamondsBall : MonoBehaviour
    {
        [SerializeField] GameObject AlphaDiamondsText;
        [SerializeField] GameObject BetaDiamondsText;
        public PhotonView View;

        private TMP_Text DiamondText;
        public GameObject TeamDiamonds;
        public TeamDiamondCount TeamDiamondCount;
        public int TeamNumber;

        private void Start()
        {
            View = transform.GetComponent<PhotonView>();
        }

        public void TeamNumberChange(int TeamNumberParam)
        {
            View.RPC("TeamNumberChangeRPC",  RpcTarget.All, TeamNumberParam);
        }

        [PunRPC]
        private void TeamNumberChangeRPC(int TeamNumberParam)
        {
            TeamNumber = TeamNumberParam;
            DiamondCounterChange();
        }

        public void DiamondCounterChange()
        {
            if (TeamNumber == 1)
            {
                TeamDiamonds = AlphaDiamondsText;
            }
            else if (TeamNumber == 2)
            {
                TeamDiamonds = BetaDiamondsText;
            }
            DiamondText = TeamDiamonds.GetComponent<TMP_Text>();
            TeamDiamondCount = TeamDiamonds.GetComponent<TeamDiamondCount>();
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        { 
            if (collision.gameObject.CompareTag("Diamond") && TeamNumber > 0)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    View.RPC("DiamondForBallRPC",  RpcTarget.All);
                    //PhotonNetwork.Destroy(collision.gameObject);
                }
                Destroy(collision.gameObject);
            }
        }

        [PunRPC]
        private void DiamondForBallRPC()
        {
            TeamDiamondCount.TeamDiamondCounter = TeamDiamondCount.TeamDiamondCounter + 1;
            DiamondText.SetText(TeamDiamondCount.TeamDiamondCounter.ToString());
        }
    }
}