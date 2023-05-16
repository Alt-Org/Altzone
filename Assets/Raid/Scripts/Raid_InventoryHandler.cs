using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_InventoryHandler : MonoBehaviour
{
    [SerializeField]
    private Raid_InventoryPage InventoryUI;

    public int InventorySize = 10;

    private void Start()
    {
        InventoryUI.InitializeInventoryUI(InventorySize);
        Debug.Log("Inventory initialized");
        InventoryUI.TestMethod();
    }
}
