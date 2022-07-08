using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Hexa_struct
{
    public enum Type
    {
        //Types of hexas on the grid
        Invalid,
        Neutral,
        Bomb,
        Number,        
    }
    public Vector3Int position;
    public Type type;
    public bool detonated;
    public bool revealed;
    public int number_of_bombs;
    public int number;
    public bool flagged;

}
