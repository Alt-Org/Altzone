using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class Field_Script : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }


    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Hexa_Struct[,] state)
    {
        int width = state.GetLength(0);//Must be 0, and int width
        int height = state.GetLength(1);//Must be 1, and int height

        for(int x = 0; x < width; x++) //int!
        {
            for (int y = 0; y < height; y++)
            {
                Hexa_Struct hexa = state[x, y];
                tilemap.SetTile(hexa.position, GetTile(hexa));
            }
        }      
    }

    private Tile GetTile(Hexa_Struct hexa)
    {
        if(hexa.revealed)
        {
            return GetRevealedTile(hexa);
        }
        else
        {
            return tileUnrevealed;
        }
    }

    private Tile GetRevealedTile(Hexa_Struct hexa)
    {
        switch(hexa.type)
        {
            case Hexa_Struct.Type.Neutral: return tileNeutral;
            case Hexa_Struct.Type.Loot: return tileLootTest; //(DEV) Loot tile for testing placement
            case Hexa_Struct.Type.Number: return LootNumberSwitch(hexa); //Return GetNumberTile when not testing lootCalculation
            case Hexa_Struct.Type.LootNumber: return HexaNumberSwitch(hexa); //Return GetLootNumberTile when not testing lootCalculation
            case Hexa_Struct.Type.Bomb: return hexa.detonated ? tileDetonated : tileBomb; //(DEV) Delete detonated if game continues after Raid!
            case Hexa_Struct.Type.NewTestBomb: return hexa.detonated ? tileDetonated : tileBomb;
            default: return null;
        }
    }

    //private Tile GetNumberTile(Hexa_Struct hexa)
    //{
    //    switch (hexa.number)
    //    {
    //        case 1: return tileNumber1;
    //        case 2: return tileNumber2;
    //        case 3: return tileNumber3;
    //        case 4: return tileNumber4;
    //        case 5: return tileNumber5;
    //        case 6: return tileNumber6;
    //        case 7: return tileNumber7;
    //        case 8: return tileNumber8;
    //        case 9: return tileNumber9;
    //        default: return null;
    //    }
    //}

    //private Tile GetLootNumberTile(Hexa_Struct hexa)
    //{
    //    switch (hexa.lootNumber)
    //    {
    //        case 1: return tileLootNumber1;
    //        case 2: return tileLootNumber2;
    //        case 3: return tileLootNumber3;
    //        case 4: return tileLootNumber4;
    //        case 5: return tileLootNumber5;
    //        case 6: return tileLootNumber6;
    //        case 7: return tileLootNumber7;
    //        case 8: return tileLootNumber8;
    //        case 9: return tileLootNumber9;
    //        default: return null;
    //    }
    //}

    private Tile LootNumberSwitch(Hexa_Struct hexa)
    {
        switch (hexa.number)
        {
            case 1: if (hexa.lootNumber > 0) { switch (hexa.lootNumber) { case 1: return tileNumber1_LootNumber1; case 2: return tileNumber1_LootNumber2; case 3: return tileNumber1_LootNumber3; case 4: return tileNumber1_LootNumber4; case 5: return tileNumber1_LootNumber5; case 6: return tileNumber1_LootNumber6; case 7: return tileNumber1_LootNumber7; case 8: return tileNumber1_LootNumber8; default: return null;  } } else return tileNumber1;
            case 2: if (hexa.lootNumber > 0) { switch (hexa.lootNumber) { case 1: return tileNumber2_LootNumber1; case 2: return tileNumber2_LootNumber2; case 3: return tileNumber2_LootNumber3; case 4: return tileNumber2_LootNumber4; case 5: return tileNumber2_LootNumber5; case 6: return tileNumber2_LootNumber6; case 7: return tileNumber2_LootNumber7; default: return null; } } else return tileNumber2;
            case 3: if (hexa.lootNumber > 0) { switch (hexa.lootNumber) { case 1: return tileNumber3_LootNumber1; case 2: return tileNumber3_LootNumber2; case 3: return tileNumber3_LootNumber3; case 4: return tileNumber3_LootNumber4; case 5: return tileNumber3_LootNumber5; case 6: return tileNumber3_LootNumber6; default: return null; } } else return tileNumber3;
            case 4: if (hexa.lootNumber > 0) { switch (hexa.lootNumber) { case 1: return tileNumber4_LootNumber1; case 2: return tileNumber4_LootNumber2; case 3: return tileNumber4_LootNumber3; case 4: return tileNumber4_LootNumber4; case 5: return tileNumber4_LootNumber5; default: return null; } } else return tileNumber4;
            case 5: if (hexa.lootNumber > 0) { switch (hexa.lootNumber) { case 1: return tileNumber5_LootNumber1; case 2: return tileNumber5_LootNumber2; case 3: return tileNumber5_LootNumber3; case 4: return tileNumber5_LootNumber4; default: return null; } } else return tileNumber5;
            case 6: if (hexa.lootNumber > 0) { switch (hexa.lootNumber) { case 1: return tileNumber6_LootNumber1; case 2: return tileNumber6_LootNumber2; case 3: return tileNumber6_LootNumber3; default: return null; } } else return tileNumber6;
            case 7: if (hexa.lootNumber > 0) { switch (hexa.lootNumber) { case 1: return tileNumber7_LootNumber1; case 2: return tileNumber7_LootNumber2; default: return null; } } else return tileNumber7;
            case 8: if (hexa.lootNumber > 0) { switch (hexa.lootNumber) { case 1: return tileNumber8_LootNumber1; default: return null; } } else return tileNumber8;
            case 9: return tileNumber9;
            default: return null;
        }
    }

    private Tile HexaNumberSwitch(Hexa_Struct hexa)
    {
        switch (hexa.lootNumber)
        {
            case 1: if (hexa.number > 0) { switch (hexa.number) { case 1: return tileNumber1_LootNumber1; case 2: return tileNumber2_LootNumber1; case 3: return tileNumber3_LootNumber1; case 4: return tileNumber4_LootNumber1; case 5: return tileNumber5_LootNumber1; case 6: return tileNumber6_LootNumber1; case 7: return tileNumber7_LootNumber1; case 8: return tileNumber8_LootNumber1; default: return null;  } } else return tileLootNumber1;
            case 2: if (hexa.number > 0) { switch (hexa.number) { case 1: return tileNumber1_LootNumber2; case 2: return tileNumber2_LootNumber2; case 3: return tileNumber3_LootNumber2; case 4: return tileNumber4_LootNumber2; case 5: return tileNumber5_LootNumber2; case 6: return tileNumber6_LootNumber2; case 7: return tileNumber7_LootNumber2; default: return null; } } else return tileLootNumber2;
            case 3: if (hexa.number > 0) { switch (hexa.number) { case 1: return tileNumber1_LootNumber3; case 2: return tileNumber2_LootNumber3; case 3: return tileNumber3_LootNumber3; case 4: return tileNumber4_LootNumber3; case 5: return tileNumber5_LootNumber3; case 6: return tileNumber6_LootNumber3; default: return null; } } else return tileLootNumber3;
            case 4: if (hexa.number > 0) { switch (hexa.number) { case 1: return tileNumber1_LootNumber4; case 2: return tileNumber2_LootNumber4; case 3: return tileNumber3_LootNumber4; case 4: return tileNumber4_LootNumber4; case 5: return tileNumber5_LootNumber4; default: return null; } } else return tileLootNumber4;
            case 5: if (hexa.number > 0) { switch (hexa.number) { case 1: return tileNumber1_LootNumber5; case 2: return tileNumber2_LootNumber5; case 3: return tileNumber3_LootNumber5; case 4: return tileNumber4_LootNumber5; default: return null; } } else return tileLootNumber5;
            case 6: if (hexa.number > 0) { switch (hexa.number) { case 1: return tileNumber1_LootNumber6; case 2: return tileNumber2_LootNumber6; case 3: return tileNumber3_LootNumber6; default: return null; } } else return tileLootNumber6;
            case 7: if (hexa.number > 0) { switch (hexa.number) { case 1: return tileNumber1_LootNumber7; case 2: return tileNumber2_LootNumber7; default: return null; } } else return tileLootNumber7;
            case 8: if (hexa.number > 0) { switch (hexa.number) { case 1: return tileNumber1_LootNumber8; default: return null; } } else return tileLootNumber8;
            case 9: return tileLootNumber9;
            default: return null;
        }
    }

    public Tile tileUnrevealed;
    public Tile tileBomb;
    public Tile tileNeutral;

    public Tile tileNumber1;
    public Tile tileNumber2;
    public Tile tileNumber3;
    public Tile tileNumber4;
    public Tile tileNumber5;
    public Tile tileNumber6;
    public Tile tileNumber7;
    public Tile tileNumber8;
    public Tile tileNumber9;

    public Tile tileLootTest;
    public Tile tileFlag;
    public Tile tileDetonated;
    public Tile tileNewTestBomb;

    //LootNumbers
    public Tile tileLootNumber1;
    public Tile tileLootNumber2;
    public Tile tileLootNumber3;
    public Tile tileLootNumber4;
    public Tile tileLootNumber5;
    public Tile tileLootNumber6;
    public Tile tileLootNumber7;
    public Tile tileLootNumber8;
    public Tile tileLootNumber9;

    //Number 1 Loot options
    public Tile tileNumber1_LootNumber1;
    public Tile tileNumber1_LootNumber2;
    public Tile tileNumber1_LootNumber3;
    public Tile tileNumber1_LootNumber4;
    public Tile tileNumber1_LootNumber5;
    public Tile tileNumber1_LootNumber6;
    public Tile tileNumber1_LootNumber7;
    public Tile tileNumber1_LootNumber8;

    //Number 2 Loot options
    public Tile tileNumber2_LootNumber1;
    public Tile tileNumber2_LootNumber2;
    public Tile tileNumber2_LootNumber3;
    public Tile tileNumber2_LootNumber4;
    public Tile tileNumber2_LootNumber5;
    public Tile tileNumber2_LootNumber6;
    public Tile tileNumber2_LootNumber7;

    //Number 3 Loot options
    public Tile tileNumber3_LootNumber1;
    public Tile tileNumber3_LootNumber2;
    public Tile tileNumber3_LootNumber3;
    public Tile tileNumber3_LootNumber4;
    public Tile tileNumber3_LootNumber5;
    public Tile tileNumber3_LootNumber6;

    //Number 4 Loot options
    public Tile tileNumber4_LootNumber1;
    public Tile tileNumber4_LootNumber2;
    public Tile tileNumber4_LootNumber3;
    public Tile tileNumber4_LootNumber4;
    public Tile tileNumber4_LootNumber5;

    //Number 5 Loot options
    public Tile tileNumber5_LootNumber1;
    public Tile tileNumber5_LootNumber2;
    public Tile tileNumber5_LootNumber3;
    public Tile tileNumber5_LootNumber4;

    //Number 6 Loot options
    public Tile tileNumber6_LootNumber1;
    public Tile tileNumber6_LootNumber2;
    public Tile tileNumber6_LootNumber3;

    //Number 7 Loot options
    public Tile tileNumber7_LootNumber1;
    public Tile tileNumber7_LootNumber2;

    //Number 8 Loot options
    public Tile tileNumber8_LootNumber1;
}
