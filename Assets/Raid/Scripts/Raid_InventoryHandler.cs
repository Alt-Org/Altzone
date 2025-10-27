using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        if (InventorySize <= 0)
        {
            int randomInventorySize = Random.Range(4, 26);
            InventorySize = randomInventorySize * 4;
        }

        CompleteStartMethod();
    }

    private void CompleteStartMethod()
    {
        if (InventoryUI == null)
        {
            Debug.LogWarning("InventoryUI reference not set on Raid_InventoryHandler.");
            return;
        }

        InventoryUI.InitializeInventoryUI(InventorySize);
        Debug.Log($"Inventory initialized with size {InventorySize}");

        InventoryUI.SetInventorySlotData();
    }
}
