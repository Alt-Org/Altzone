using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_InventoryHandler : MonoBehaviour
{
    [SerializeField, Header("Assigned scripts")]
    private Raid_InventoryPage InventoryUI;

    [SerializeField, Header("Variables")]
    public int InventorySize;
    public int MediumItemMaxAmount;
    public int LargeItemMaxAmount;

    private void Start()
    {
        InventoryUI.InitializeInventoryUI(InventorySize);
        Debug.Log("Inventory initialized");
        InventoryUI.SetInventorySlotData(InventorySize);
    }
}
