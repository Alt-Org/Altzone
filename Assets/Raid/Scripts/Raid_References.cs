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
    [SerializeField] private Raid_EventLog eventLog;
    [SerializeField] private Raid_Timer raidTimer;

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

    public Raid_EventLog EventLog
    {
        get
        {
            ResolveEventLog();
            return eventLog;
        }
    }

    public Raid_Timer RaidTimer
    {
        get
        {
            ResolveRaidTimer();
            return raidTimer;
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
        RaidTimer?.HideRaidControlsForEndMenu();

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
        ResolveEventLog();
        ResolveRaidTimer();
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

    private void ResolveEventLog()
    {
        if (eventLog != null)
        {
            return;
        }

        eventLog = GetComponentInChildren<Raid_EventLog>(true);
        if (eventLog == null)
        {
            eventLog = FindObjectOfType<Raid_EventLog>(true);
        }
    }

    private void ResolveRaidTimer()
    {
        if (raidTimer != null)
        {
            return;
        }

        raidTimer = FindObjectOfType<Raid_Timer>();
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
