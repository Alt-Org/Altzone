using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Hexa_struct
{
    public enum Type
    {
        //Types of hexas on the grid

        Empty,
        Bomb,
        Number,
    }
    public Vector3Int position;
    public Type type;
    public bool detonated;
    public bool hidden;
    public int number_of_bombs;

}
