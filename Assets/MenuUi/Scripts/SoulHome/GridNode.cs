using System.Collections;
using System.Collections.Generic;
using MenuUI.Scripts.SoulHome;
using UnityEngine;

public class GridNode
{
    public Vector2Int GridPosition;
    public bool IsFurniture;
    public bool IsBackSlot; // last row of slots on a specific furniture
    public int penalty;
    public FurnitureSlot FurnitureSlot;

    public float GCost = float.MaxValue;
    public float HCost;
    public float FCost => GCost + HCost;

    public GridNode Parent;

    public GridNode(Vector2Int gridPos, bool isFurniture, bool isBackSlot)
    {
        GridPosition = gridPos;
        IsFurniture = isFurniture;
        IsBackSlot = isBackSlot;
    }

    public void Reset()
    {
        GCost = float.MaxValue;
        HCost = 0f;
        Parent = null;
    }
}
