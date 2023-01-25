using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()  //public override void
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()    //public override void
    {
        SceneManager.LoadScene("Lobby");
    }
}
