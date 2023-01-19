using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_Grid : MonoBehaviour
{
    public int AmountOfMines;
    public GameObject[,] grid = new GameObject[9,9];

    private void Start()
    {
        for (int i = 0; i < AmountOfMines; i++)
        {
            PlaceMines();
        }

        PlaceNumberTiles();
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
            GameObject MineTile = Instantiate(Resources.Load("Prefabs/Mine", typeof(GameObject)), new Vector3(x, y, 0), Quaternion.identity) as GameObject;

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
                        if (grid[x, y + 1] != null)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x+1 < 9)
                    {
                        if (grid[x+1, y] != null)
                        {
                            NearbyMines++;
                        }
                    }

                    if (y-1 >= 0)
                    {
                        if (grid[x, y-1] != null)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x - 1 >= 0)
                    {
                        if (grid[x-1, y] != null)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x+1 < 9 && y+1 < 9)
                    {
                        if (grid[x+1, y+1] != null)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x + 1 < 9 && y - 1 >= 0)
                    {
                        if (grid[x+1, y-1] != null)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x - 1 >= 0 && y - 1 >= 0)
                    {
                        if (grid[x-1, y-1] != null)
                        {
                            NearbyMines++;
                        }
                    }

                    if (x - 1 >= 0 && y + 1 < 9)
                    {
                        if (grid[x-1, y+1] != null)
                        {
                            NearbyMines++;
                        }
                    }

                    if (NearbyMines > 0)
                    {
                        GameObject NumberTile = Instantiate(Resources.Load("Prefabs/" + NearbyMines, typeof(GameObject)), new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                    }
                }
            }
        }
    }

}
