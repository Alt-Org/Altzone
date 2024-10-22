using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Battle1.Scripts.Test.Elmeri
{
    public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
    {
        public TMP_InputField CreateInput;
        public TMP_InputField JoinInput;

        public void CreateRoom()
        {
            PhotonNetwork.CreateRoom(CreateInput.text);
        }

        public void JoinRoom()
        {
            Debug.Log("kojok");
            PhotonNetwork.JoinRoom(JoinInput.text);
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.LoadLevel("te-test-physics-ball");
        }
    }
}
