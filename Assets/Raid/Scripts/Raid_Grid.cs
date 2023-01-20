using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Raid_Grid : MonoBehaviour
{
    public int AmountOfMines;
    public Raid_Tile[,] grid = new Raid_Tile[9,9];

    private void Start()
    {
        for (int i = 0; i < AmountOfMines; i++)
        {
            PlaceMines();
        }

        PlaceNumberTiles();
        PlaceEmptyTiles();
    }

    private void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            int x = Mathf.RoundToInt(mousePosition.x);
            int y = Mathf.RoundToInt(mousePosition.y);

            Raid_Tile raid_Tile = grid[x,y];

            raid_Tile.SetIsCovered(false);
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

}
