using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DiamondDisappear : MonoBehaviour
{
    [SerializeField] PhotonView View;
    public static bool Master = false;
    public List<GameObject> DiamondsArray = new List<GameObject>();

    public void Start()
    {
        if (PhotonNetwork.IsMasterClient)   
        { 
            Master = true;
        }
    }

    public void DiamondDestroy()
    {
        View.RPC("DisappearRPC",  RpcTarget.All);
    }

    [PunRPC]
    private void DisappearRPC()
    {
        //Removes all null objects from the list
        DiamondsArray.RemoveAll(GameObject => GameObject == null);
        if (DiamondsArray.Count > 0)
        {
            Destroy(DiamondsArray[0]);
            DiamondsArray.Remove(DiamondsArray[0]);
        }
    }
}
