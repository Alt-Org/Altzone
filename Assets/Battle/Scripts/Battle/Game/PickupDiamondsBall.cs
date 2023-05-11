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
        [SerializeField] GameObject AlphaDiamondsText2;
        [SerializeField] GameObject BetaDiamondsText2;
        public PhotonView View;

        private TMP_Text DiamondText;
        public GameObject TeamDiamonds;
        public TeamDiamondCount TeamDiamondCount;
        private TMP_Text DiamondText2;
        public GameObject TeamDiamonds2;
        public TeamDiamondCount TeamDiamondCount2;
        public int TeamNumber;

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
                TeamDiamonds2 = AlphaDiamondsText2;
            }
            else if (TeamNumber == 2)
            {
                TeamDiamonds = BetaDiamondsText;
                TeamDiamonds2 = BetaDiamondsText2;
            }
            DiamondText = TeamDiamonds.GetComponent<TMP_Text>();
            TeamDiamondCount = TeamDiamonds.GetComponent<TeamDiamondCount>();
            DiamondText2 = TeamDiamonds2.GetComponent<TMP_Text>();
            TeamDiamondCount2 = TeamDiamonds2.GetComponent<TeamDiamondCount>();
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        { 
            if (collision.gameObject.CompareTag("Diamond") && TeamNumber > 0)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    int DiamondType = 1;
                    View.RPC("DiamondForBallRPC",  RpcTarget.All, DiamondType);
                }
                Destroy(collision.gameObject);
            }
            if (collision.gameObject.CompareTag("Diamond2") && TeamNumber > 0)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    int DiamondType = 2;
                    View.RPC("DiamondForBallRPC",  RpcTarget.All, DiamondType);
                }
                Destroy(collision.gameObject);
            }
        }

        [PunRPC]
        private void DiamondForBallRPC(int DiamondType)
        {
            if (DiamondType == 1)
            {
                TeamDiamondCount.TeamDiamondCounter = TeamDiamondCount.TeamDiamondCounter + 1;
                DiamondText.SetText(TeamDiamondCount.TeamDiamondCounter.ToString());
            }
            if (DiamondType == 2)
            {
                TeamDiamondCount2.TeamDiamondCounter = TeamDiamondCount2.TeamDiamondCounter + 1;
                DiamondText2.SetText(TeamDiamondCount2.TeamDiamondCounter.ToString());
            }
        }
    }
}