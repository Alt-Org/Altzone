using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_Grid : MonoBehaviour
{
    public static int Gridrows;
    public static int Gridcolumns;
    public GameObject[,] grid = new GameObject[9,9];

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            PlaceMines();
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
            GameObject empty = new GameObject();

            grid[x,y] = empty;
            Debug.Log("(" + x + ", " + y + ")");
        }
        else
        {
            PlaceMines();
        }
    }

}
