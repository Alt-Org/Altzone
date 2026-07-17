using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_References : MonoBehaviour
{
    [SerializeField, Header("Reference GameObjects")]
 
    public GameObject EndMenu;
    public GameObject HeartHalves;
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

    private void Start()
    {
        inventoryHandler = GameObject.Find("ScriptHolder").GetComponent<Raid_InventoryHandler>();

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
            endMenuController = EndMenu.GetComponent<Raid_EndMenu>();
        }

        if (EndMenu == null && endMenuController != null)
        {
            EndMenu = endMenuController.MenuRoot;
        }
    }
}
