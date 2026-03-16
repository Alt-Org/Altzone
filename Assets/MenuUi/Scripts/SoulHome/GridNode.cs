using System.Collections;
using System.Collections.Generic;
using MenuUI.Scripts.SoulHome;
using UnityEngine;

public class GridNode
{
    public Vector2Int GridPosition;
    public bool IsFurniture;
    public int penalty;
    public FurnitureSlot FurnitureSlot;

    public float GCost = float.MaxValue;
    public float HCost;
    public float FCost => GCost + HCost;

    public GridNode Parent;

    public GridNode(Vector2Int gridPos, bool isFurniture)
    {
        GridPosition = gridPos;
        IsFurniture = isFurniture;
    }

    public void Reset()
    {
        GCost = float.MaxValue;
        HCost = 0f;
        Parent = null;
    }
}
