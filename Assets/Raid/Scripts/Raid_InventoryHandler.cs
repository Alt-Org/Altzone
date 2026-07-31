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
        if (RaidMatchmakingController.Instance != null && RaidMatchmakingController.Instance.ControlsInventorySetup)
        {
            return;
        }

        int randomInventorySize = Random.Range(6, 12);
        InventorySize = randomInventorySize*4;
        InventoryUI.InitializeInventoryUI(InventorySize);
        InventoryUI.RandomizeInventoryContent(InventorySize);

    }

    public void InitializeSharedInventory(int inventorySize, int seed, RaidPhotonRoom.TrapData[] traps)
    {
        InventorySize = inventorySize;
        InventoryUI.InitializeInventoryUI(InventorySize, traps);
        InventoryUI.RandomizeInventoryContentDeterministic(InventorySize, seed);
    }

}
