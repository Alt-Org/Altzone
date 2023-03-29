using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_Slot : MonoBehaviour
{
    public enum SlotType
    {
        Empty, Occupied
    }

    public enum SlotWeight
    {
        Zero, Five, Ten, Fifteen, Twenty, TwentyFive
    }

    public bool IsOccupied = false;
    public Sprite EmptySlot;

    public SlotType slotType = SlotType.Empty;
    public SlotWeight slotWeight = SlotWeight.Zero;

    private Sprite DefaultInventorySlot;

    private void Start()
    {
        DefaultInventorySlot = GetComponent<SpriteRenderer>().sprite;

        GetComponent<SpriteRenderer>().sprite = EmptySlot;
    }

    public void SetIsCovered(bool Covered)
    {
        IsOccupied = false;
        GetComponent<SpriteRenderer>().sprite = DefaultInventorySlot;
    }
}
