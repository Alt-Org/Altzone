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
        public GameObject TeamDiamonds;
        [SerializeField] GameObject AlphaDiamondsText;
        [SerializeField] GameObject BetaDiamondsText;

        private TMP_Text DiamondText;
        public TeamDiamondCount TeamDiamondCount;
        //public PhotonView View;
        public int TeamNumber;

        /*public void TeamNumberChange()
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
            if (collision.gameObject.CompareTag("Diamond"))
            {
                TeamDiamondCount.TeamDiamondCounter = TeamDiamondCount.TeamDiamondCounter + 1;
                DiamondText.SetText(TeamDiamondCount.TeamDiamondCounter.ToString());
                Destroy(collision.gameObject);
            }
        }*/
    }
}