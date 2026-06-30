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
    [SerializeField] private Raid_LiveInventory liveInventory;

    public static Raid_References Instance { get; private set; }

    public Raid_InventoryHandler InventoryHandler
    {
        get
        {
            ResolveInventoryHandler();
            return inventoryHandler;
        }
    }

    public Raid_EndMenu EndMenuController
    {
        get
        {
            ResolveEndMenuController();
            return endMenuController;
        }
    }

    public Raid_LootTracking LootTracking
    {
        get
        {
            ResolveLootTracking();
            return raid_LootTracking;
        }
    }

    public Raid_LiveInventory LiveInventory
    {
        get
        {
            ResolveLiveInventory();
            return liveInventory;
        }
    }

    private void Awake()
    {
        RegisterInstance();
        ResolveReferences();
    }

    private void OnEnable()
    {
        RegisterInstance();
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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
        ResolveInventoryHandler();
        ResolveLootTracking();
        ResolveEndMenuController();
    }

    private void ResolveInventoryHandler()
    {
        if (inventoryHandler != null)
        {
            return;
        }

        if (!TryGetComponent(out inventoryHandler))
        {
            inventoryHandler = FindObjectOfType<Raid_InventoryHandler>();
        }
    }

    private void ResolveLootTracking()
    {
        if (raid_LootTracking != null)
        {
            return;
        }

        if (!TryGetComponent(out raid_LootTracking))
        {
            raid_LootTracking = FindObjectOfType<Raid_LootTracking>();
        }
    }

    private void ResolveLiveInventory()
    {
        if (liveInventory != null)
        {
            return;
        }

        liveInventory = Raid_LiveInventory.Instance;
        if (liveInventory != null)
        {
            return;
        }

        liveInventory = FindObjectOfType<Raid_LiveInventory>(true);
        if (liveInventory != null)
        {
            return;
        }

        Raid_LiveInventory prefab = Resources.Load<Raid_LiveInventory>("Prefabs/LiveInventory");
        if (prefab == null)
        {
            Debug.LogError("Cannot open raid live inventory because Prefabs/LiveInventory is missing.");
            return;
        }

        Transform parent = EndMenu != null && EndMenu.transform.parent != null
            ? EndMenu.transform.parent
            : transform.root;

        liveInventory = Instantiate(prefab, parent, false);
        liveInventory.name = "LiveInventory";
    }

    private void RegisterInstance()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
            return;
        }

        if (!Instance.gameObject.activeInHierarchy && gameObject.activeInHierarchy)
        {
            Instance = this;
        }
    }
}
