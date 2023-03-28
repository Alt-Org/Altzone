using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_Inventory : MonoBehaviour
{
    public Raid_Slot[,] Inventory = new Raid_Slot[4, 6];

    private Transform _transform;

    [Header("Inventory Content")]
    public int EmptySlotAmount;

    private void Start()
    {
        for (int i = 0; i < EmptySlotAmount; i++)
        {
            PlaceEmptySlots();
        }
    }
    void PlaceEmptySlots()
    {
        int x = Random.Range(0, 4);
        int y = Random.Range(0, 6);

        if (Inventory[x, y] == null)
        {
            Raid_Slot EmptySlot = Instantiate(Resources.Load("Prefabs/EmptySlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;

            Inventory[x, y] = EmptySlot;
            Debug.Log("Inventory slot (" + x + ", " + y + ") is empty.");
        }
        else
        {
            PlaceEmptySlots();
        }
    }
}
