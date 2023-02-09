using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class Raid_Grid : MonoBehaviour
{
    public Sprite CoveredTile;
    public Sprite FlagTile;

    public int AmountOfMines;
    public int AmountofSingles;
    public int AmountofDoubles;
    public int AmountofTriples;

    public Raid_Tile[,] grid = new Raid_Tile[9,9];

    public List<Raid_Tile> TilesToCheck = new List<Raid_Tile>();

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        for (int i = 0; i < AmountOfMines; i++)
        {
            PlaceMines();
        }
        for (int i = 0; i < AmountofSingles; i++)
        {
            PlaceSingleTileFurniture();
        }
        for (int i = 0; i < AmountofDoubles; i++)
        {
            PlaceDoubleTileFurniture();
        }
        for (int i = 0; i < AmountofTriples; i++)
        {
            PlaceTripleTileFurniture();
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
                    raid_Tile.GetComponent<SpriteRenderer>().sprite = FlagTile;
                }
                else
                {
                    raid_Tile.tileState = Raid_Tile.TileState.Normal;
                    raid_Tile.GetComponent<SpriteRenderer>().sprite = CoveredTile;
                }
            }
        }
    }

    void PlaceSingleTileFurniture()
    {
        int x = UnityEngine.Random.Range(0, 9);
        int y = UnityEngine.Random.Range(0, 9);

        if (grid[x, y] == null)
        {
            Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Single", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;

            grid[x, y] = FurnitureTile;

            Debug.Log("Single furniture spawn at (" + x + ", " + y + ")");
        }
        else
        {
            Debug.Log("Single furniture can't be placed on a non-vacant tile");
            PlaceSingleTileFurniture();
        }
    }

    void PlaceDoubleTileFurniture()
    {
        int x = UnityEngine.Random.Range(0, 9);
        int y = UnityEngine.Random.Range(0, 9);

        if (grid[x, y] == null)
        {
            if (x+1 < 9 && grid[x+1, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;

                Debug.Log("Double furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Double furniture spawn at (" + (x+1) + ", " + y + ")");

            }
            else if (y+1 < 9 && grid[x, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;

                Debug.Log("Double furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Double furniture spawn at (" + x + ", " + (y+1) + ")");
            }
            else if (x-1 >= 0 && grid[x-1, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x-1, y] = FurnitureTile2;

                Debug.Log("Double furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Double furniture spawn at (" + (x-1) + ", " + y + ")");
            }
            else if (y-1 >= 0 && grid[x, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x, y-1] = FurnitureTile2;

                Debug.Log("Double furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Double furniture spawn at (" + x + ", " + (y-1) + ")");
            }
            else
            {
                PlaceDoubleTileFurniture();
            }      
        }
        else
        {
            Debug.Log("Double furniture can't be placed on a non-vacant tile");
            PlaceDoubleTileFurniture();
        }
    }

    void PlaceTripleTileFurniture()
    {
        int x = UnityEngine.Random.Range(0, 9);
        int y = UnityEngine.Random.Range(0, 9);

        if (grid[x, y] == null)
        {
            if (x+2 < 9 && grid[x+1, y] == null && grid[x+2, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+2, y, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x+2, y] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+2) + ", " + y + ")");
            }
            else if (x+1 < 9 && x-1 >= 0 && grid[x+1, y] == null && grid[x-1, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x-1, y] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + y + ")");
            }
            else if (x-2 >= 0 && grid[x-1, y] == null && grid[x-2, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-2, y, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x-1, y] = FurnitureTile2;
                grid[x-2, y] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-2) + ", " + y + ")");
            }
            else if (y+2 < 9 && grid[x, y+1] == null && grid[x, y+2] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+2, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;
                grid[x, y+2] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+2) + ")");
            }
            else if (y+1 < 9 && y-1 >= 0 && grid[x, y+1] == null && grid[x, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;
                grid[x, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
            }
            else if (y-2 >= 0 && grid[x, y-1] == null && grid[x, y-2] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-2, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x, y-1] = FurnitureTile2;
                grid[x, y-2] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-2) + ")");
            }
            else if (x+1 < 9 && y+1 < 9 && grid[x+1, y] == null && grid[x+1, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y+1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x+1, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + (y+1) + ")");
            }
            else if (x+1 < 9 && y-1 >= 0 && grid[x+1, y] == null && grid[x+1, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y-1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x+1, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + (y-1) + ")");
            }
            else if (x+1 < 9 && y+1 < 9 && grid[x+1, y] == null && grid[x, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
            }
            else if (x+1 < 9 && y-1 >= 0 && grid[x+1, y] == null && grid[x, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
            }
            else if (y+1 < 9 && x+1 < 9 && grid[x, y+1] == null && grid[x+1, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y+1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;
                grid[x+1, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + (y+1) + ")");
            }
            else if (y+1 < 9 && x-1 >= 0 && grid[x, y+1] == null && grid[x-1, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y+1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;
                grid[x-1, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + (y+1) + ")");
            }
            else if (y-1 >= 0 && x+1 < 9 && grid[x, y-1] == null && grid[x+1, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y-1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x, y-1] = FurnitureTile2;
                grid[x+1, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + (y-1) + ")");
            }
            else if (y-1 >= 0 && x-1 >= 0 && grid[x, y-1] == null && grid[x-1, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y-1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x, y-1] = FurnitureTile2;
                grid[x-1, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + (y-1) + ")");
            }
            else if (x-1 >= 0 && y+1 < 9 && grid[x-1, y] == null && grid[x-1, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y+1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x-1, y] = FurnitureTile2;
                grid[x-1, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + (y+1) + ")");
            }
            else if (x-1 >= 0 && y-1 >= 0 && grid[x-1, y] == null && grid[x-1, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y-1, 0), Quaternion.identity) as Raid_Tile;

                grid[x, y] = FurnitureTile;
                grid[x-1, y] = FurnitureTile2;
                grid[x-1, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + (y-1) + ")");
            }
        }
        else
        {
            Debug.Log("Triple furniture can't be placed on a non-vacant tile");
            PlaceTripleTileFurniture();
        }
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
