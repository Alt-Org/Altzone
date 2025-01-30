#if false
/*using Battle1.PhotonRealtime.Code;*/
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
/*using IOnEventCallback = Battle1.PhotonRealtime.Code.IOnEventCallback;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using ReceiverGroup = Battle1.PhotonRealtime.Code.ReceiverGroup;*/

namespace Battle1.Scripts.Test.Elmeri
{
    public class RaiseEventTest : MonoBehaviour, IOnEventCallback
    {
        // If you have multiple custom events, it is recommended to define them in the used class
        public const byte MoveUnitsToTargetPositionEventCode = 1;

        private void Start()
        {
            SendMoveUnitsToTargetPositionEvent();
        }

        private void SendMoveUnitsToTargetPositionEvent()
        {
            object[] content = new object[] { "new Vector3(10.0f, 2.0f, 5.0f), 1, 2, 5,1 0" }; // Array contains the target position and the IDs of the selected units
            /*RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent(MoveUnitsToTargetPositionEventCode, content, raiseEventOptions, SendOptions.SendReliable);      //content, 
*/        }

        private void OnEnable()
        {
            /*PhotonNetwork.AddCallbackTarget(this);*/
        }

        private void OnDisable()
        {
            /*PhotonNetwork.RemoveCallbackTarget(this);*/
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            if (eventCode == MoveUnitsToTargetPositionEventCode)
            {
                Debug.Log("lplplplpl");
            }
        }
    }
}
#endif
