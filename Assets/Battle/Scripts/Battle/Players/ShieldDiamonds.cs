using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle.Scripts.Battle.Game;

namespace Battle.Scripts.Battle.Players
{
    internal class ShieldDiamonds : MonoBehaviour
    {
        public Pickup Pickup;
        private int TeamNumber;
        /*public PickupDiamondsBall PickupDiamondsBall;
        public bool StartBool = true;

        void Start()
        {
            TeamNumber = Pickup.TeamNumber;
            PickupDiamondsBall = GameObject.FindGameObjectWithTag("Ball").GetComponent<PickupDiamondsBall>();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            var otherGameObject = collider.gameObject;
            if (otherGameObject.CompareTag("Ball"))
            {
                //if (StartBool == true)
                //{
                //    PickupDiamondsBall = collider.gameObject.GetComponent<PickupDiamondsBall>();        //otherGameObject.
                //    StartBool = false;
                //}
                if (PickupDiamondsBall.TeamNumber != TeamNumber)
                {
                    PickupDiamondsBall.TeamNumber = TeamNumber;
                    PickupDiamondsBall.TeamNumberChange();
                }
            }
        }*/
    }
}
