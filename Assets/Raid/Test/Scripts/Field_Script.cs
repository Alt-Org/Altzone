using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Field_Script : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Hexa_struct[,] state)
    {
        int width = state.GetLength(0);
        int height = state.GetLength(1);

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Hexa_struct hexa = state[x, y];
            }
        }      
    }

    public Tile tileUnrevealed;
    public Tile tileBomb;
    public Tile tileEmpty;
    public Tile tileNumber1;
    public Tile tileNumber2;
    public Tile tileNumber3;
    public Tile tileNumber4;
    public Tile tileNumber5;
    public Tile tileNumber6;
}
