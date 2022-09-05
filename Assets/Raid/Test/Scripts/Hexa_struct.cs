using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Hexa_Struct
{
    public enum Type
    {
        //Types of hexas on the grid
        Invalid,
        Neutral,
        Bomb, //Bomb type
        Number, //Numbers for use when only bombs are near
        Loot, //Loot type
        NewTestBomb,//delete if does not work
        LootNumber, //Numbers for use when only loot is near
        NumberAndLootNumber, //Numbers for use when bombs AND loot is near
    }
    public Vector3Int position; //must be Vector3Int
    public Type type;
    public bool detonated;
    public bool revealed;
    public int number_of_bombs;
    public int number;
    public int lootNumber;
    public bool flagged;

}
