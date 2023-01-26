using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Raid_Grid : MonoBehaviour
{
    public int AmountOfMines;

    public Raid_Tile[,] grid = new Raid_Tile[9,9];

    public List<Raid_Tile> TilesToCheck = new List<Raid_Tile>();

    private void Start()
    {
        for (int i = 0; i < AmountOfMines; i++)
        {
            PlaceMines();
        }

        PlaceNumberTiles();
        PlaceEmptyTiles();
    }

    public void CheckInputQuickTap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            int x = Mathf.RoundToInt(mousePosition.x);
            int y = Mathf.RoundToInt(mousePosition.y);

            Raid_Tile raid_Tile = grid[x,y];

            if(raid_Tile.tileState == Raid_Tile.TileState.Normal)
            {
                if(raid_Tile.IsCovered)
                {
                    raid_Tile.SetIsCovered(false);

                    if (raid_Tile.tileType == Raid_Tile.TileType.Empty)
                    {
                        RevealAdjacentTilesForTileAt(x, y);
                    }
                }
            }
            Debug.Log("QuickTap recognized at (" + x + ", " + y + ")");
        }
    }

    public void CheckInputSlowTap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            int x = Mathf.RoundToInt(mousePosition.x);
            int y = Mathf.RoundToInt(mousePosition.y);

            Raid_Tile raid_Tile = grid[x, y];
            Debug.Log("SlowTap recognized at (" + x + ", " + y + ")");
            if (raid_Tile.IsCovered)
            {
                if (raid_Tile.tileState == Raid_Tile.TileState.Normal)
                {
                    raid_Tile.tileState = Raid_Tile.TileState.Flagged;
                }
                else
                {
                    raid_Tile.tileState = Raid_Tile.TileState.Normal;
                }
            }
        }
    }

    void PlaceFurniture()
    {

    }

    void PlaceMines()
    {
        int x = UnityEngine.Random.Range(0, 9);
        int y = UnityEngine.Random.Range(0, 9);

        if(grid[x,y] == null)
        {
            Raid_Tile MineTile = Instantiate(Resources.Load("Prefabs/Mine", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;

            grid[x,y] = MineTile;
            Debug.Log("(" + x + ", " + y + ")");
        }
        else
        {
            PlaceMines();
        }
    }

    void PlaceNumberTiles()
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (grid[x,y] == null)
                {
                    // Tile is empty and thus can't be a mine.

                    // Check nearby cells for mines (up, right, down, left and corners)

                    int NearbyMines = 0;

                    if (y+1 < 9)
                    {
                        if (grid[x, y + 1] != null && grid[x, y+1].tileType == Raid_Tile.TileType.Mine)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x+1 < 9)
                    {
                        if (grid[x+1, y] != null && grid[x+1, y].tileType == Raid_Tile.TileType.Mine)
                        {
                            NearbyMines++;
                        }
                    }

                    if (y-1 >= 0)
                    {
                        if (grid[x, y-1] != null && grid[x, y-1].tileType == Raid_Tile.TileType.Mine)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x - 1 >= 0)
                    {
                        if (grid[x-1, y] != null && grid[x-1, y].tileType == Raid_Tile.TileType.Mine)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x+1 < 9 && y+1 < 9)
                    {
                        if (grid[x+1, y+1] != null && grid[x+1, y+1].tileType == Raid_Tile.TileType.Mine)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x + 1 < 9 && y - 1 >= 0)
                    {
                        if (grid[x+1, y-1] != null && grid[x+1, y-1].tileType == Raid_Tile.TileType.Mine)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x - 1 >= 0 && y - 1 >= 0)
                    {
                        if (grid[x-1, y-1] != null && grid[x-1, y-1].tileType == Raid_Tile.TileType.Mine)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x - 1 >= 0 && y + 1 < 9)
                    {
                        if (grid[x-1, y+1] != null && grid[x-1, y+1].tileType == Raid_Tile.TileType.Mine)
                        {
                            NearbyMines++;
                        }
                    }

                    if (NearbyMines > 0)
                    {
                        Raid_Tile NumberTile = Instantiate(Resources.Load("Prefabs/" + NearbyMines, typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;

                        grid[x, y] = NumberTile;
                    }
                }
            }
        }
    }

    void PlaceEmptyTiles()
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (grid[x, y] == null)
                {
                    Raid_Tile EmptyTile = Instantiate(Resources.Load("Prefabs/Empty_Tile", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                    grid[x, y] = EmptyTile;
                }
            }
        }
    }

    void RevealAdjacentTilesForTileAt (int x, int y)
    {
        // Check all directions to the end of the grid.

        if ((y+1) < 9)
        {
            CheckTileAt(x, y+1);
        }

        if ((x+1) < 9)
        {
            CheckTileAt(x+1, y);
        }

        if ((y-1) >= 0)
        {
            CheckTileAt(x, y-1);
        }

        if ((x-1) >= 0)
        {
            CheckTileAt(x-1, y);
        }

        if ((x+1) < 9 && (y+1) < 9)
        {
            CheckTileAt(x+1, y+1);
        }

        if ((x+1) < 9 && (y-1) >= 0)
        {
            CheckTileAt(x+1, y-1);
        }

        if ((x-1) > 0 && (y-1) >= 0)
        {
            CheckTileAt(x-1, y-1);
        }

        if ((x-1) >= 0 && (y+1) < 9)
        {
            CheckTileAt(x-1, y+1);
        }

        for (int i = TilesToCheck.Count - 1; i >= 0; i--)
        {
            if (TilesToCheck[i].DidCheck)
            {
                TilesToCheck.RemoveAt(i);
            }
        }

        if (TilesToCheck.Count > 0)
        {
            RevealAdjacentTilesForTiles();
        }
    }

    private void RevealAdjacentTilesForTiles()
    {
        for ( int i = 0; i < TilesToCheck.Count; i++)
        {
            Raid_Tile raid_Tile = TilesToCheck[i];

            int x = (int)raid_Tile.gameObject.transform.localPosition.x;
            int y = (int)raid_Tile.gameObject.transform.localPosition.y;

            raid_Tile.DidCheck = true;
            raid_Tile.SetIsCovered(false);

            RevealAdjacentTilesForTileAt(x, y);
        }
    }

    private void CheckTileAt(int x, int y)
    {
        Raid_Tile raid_Tile = grid[x, y];

        if (raid_Tile.tileType == Raid_Tile.TileType.Empty)
        {
            TilesToCheck.Add(raid_Tile);
            Debug.Log("Tile at (" + x + ", " + y + ") is an Empty tile");
        }
        else if (raid_Tile.tileType == Raid_Tile.TileType.Mine)
        {
            Debug.Log("Tile at (" + x + ", " + y + ") is a Mine tile");
        }
        else if (raid_Tile.tileType == Raid_Tile.TileType.Number)
        {
            raid_Tile.SetIsCovered(false);
            Debug.Log("Tile at (" + x + ", " + y + ") is a Number tile");
        }
        else if (raid_Tile.tileType == Raid_Tile.TileType.Furniture)
        {
            Debug.Log("Tile at (" + x + ", " + y + ") is a Furniture tile");
        }
    }
}
