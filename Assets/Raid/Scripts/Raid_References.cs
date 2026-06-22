using UnityEngine;

[RequireComponent(typeof(RaidMatchmakingController))]
public class Raid_References : MonoBehaviour
{
    [SerializeField, Header("Reference GameObjects")]
 
    public GameObject EndMenu;
    public GameObject Heart;

    [SerializeField, Header("Reference game components")]
    public Raid_InventoryHandler inventoryHandler;
    public Raid_LootTracking raid_LootTracking;
    [SerializeField] private Raid_EndMenu endMenuController;

    public Raid_EndMenu EndMenuController
    {
        get
        {
            ResolveEndMenuController();
            return endMenuController;
        }
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();

        HideEndMenu();
    }

    public void ShowEndMenu()
    {
        if (EndMenuController != null)
        {
            EndMenuController.Show();
        }
        else if (EndMenu != null)
        {
            EndMenu.SetActive(true);
        }
    }

    public void HideEndMenu()
    {
        if (EndMenuController != null)
        {
            EndMenuController.Hide();
        }
        else if (EndMenu != null)
        {
            EndMenu.SetActive(false);
        }
    }

    private void ResolveEndMenuController()
    {
        if (endMenuController == null)
        {
            endMenuController = Raid_EndMenu.Instance;
        }

        if (endMenuController == null && EndMenu != null)
        {
            EndMenu.TryGetComponent(out endMenuController);
        }

        if (EndMenu == null && endMenuController != null)
        {
            EndMenu = endMenuController.MenuRoot;
        }
    }

    private void ResolveReferences()
    {
        if (inventoryHandler == null)
        {
            TryGetComponent(out inventoryHandler);
        }

        if (inventoryHandler == null)
        {
            inventoryHandler = FindObjectOfType<Raid_InventoryHandler>();
        }

        if (raid_LootTracking == null)
        {
            TryGetComponent(out raid_LootTracking);
        }

        if (raid_LootTracking == null)
        {
            raid_LootTracking = FindObjectOfType<Raid_LootTracking>();
        }

        ResolveEndMenuController();
    }
}
