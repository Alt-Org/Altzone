using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
using Random = UnityEngine.Random;

public class Raid_InventoryHandler : MonoBehaviour
{
    [SerializeField, Header("Assigned scripts")]
    private Raid_InventoryPage InventoryUI;

    [SerializeField, Header("Inventory content")]
    public int InventorySize;
    public int MediumItemMaxAmount;
    public int LargeItemMaxAmount;

    /*public PhotonView _photonView { get; private set; }

    private void Start()
    {
        _photonView = gameObject.AddComponent<PhotonView>();
        _photonView.ViewID = 1;
        if (PhotonNetwork.IsMasterClient)
        {
            int randomInventorySize = Random.Range(4, 26);
            _photonView.RPC(nameof(SetRandomInventorySizeRPC), RpcTarget.All, randomInventorySize);
        }
    }

    [PunRPC]
    public void SetRandomInventorySizeRPC(int inventorySize)
    {
        InventorySize = inventorySize*4;
        CompleteStartMethod();
    }

    private void CompleteStartMethod()
    {
        InventoryUI.InitializeInventoryUI(InventorySize);
        Debug.Log("Inventory initialized");
        if (PhotonNetwork.IsMasterClient)
            InventoryUI.SetInventorySlotData(InventorySize);
    }*/


}
