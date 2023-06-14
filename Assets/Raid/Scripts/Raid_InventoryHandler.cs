using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Raid_InventoryHandler : MonoBehaviour
{
    [SerializeField, Header("Assigned scripts")]
    private Raid_InventoryPage InventoryUI;

    [SerializeField, Header("Inventory content")]
    public int InventorySize;
    public int MediumItemMaxAmount;
    public int LargeItemMaxAmount;

    private void Start()
    {
        InventoryUI.InitializeInventoryUI(InventorySize);
        Debug.Log("Inventory initialized");
        if (PhotonNetwork.IsMasterClient)
            InventoryUI.SetInventorySlotData(InventorySize);
    }
}
