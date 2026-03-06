using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    public Vector2Int GridPosition;
    public bool IsWalkable;
    public int penalty;

    public float GCost = float.MaxValue;
    public float HCost;
    public float FCost => GCost + HCost;

    public GridNode Parent;

    public GridNode(Vector2Int gridPos, bool isWalkable)
    {
        GridPosition = gridPos;
        IsWalkable = isWalkable;
    }

    public void Reset()
    {
        GCost = float.MaxValue;
        HCost = 0f;
        Parent = null;
    }
}
