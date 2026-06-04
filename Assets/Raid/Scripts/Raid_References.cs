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

    private void Start()
    {
        inventoryHandler = GameObject.Find("ScriptHolder").GetComponent<Raid_InventoryHandler>();
   
        if (EndMenu != null)
        {
            EndMenu.SetActive(false);
        }
    }
}
