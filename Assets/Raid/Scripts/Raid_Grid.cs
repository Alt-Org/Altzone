using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using TMPro;

public class Raid_Grid : MonoBehaviour
{
    public Sprite CoveredTile;
    public Sprite FlagTile;
    public Sprite SFurnitureSprite0;
    public Sprite SFurnitureSprite1;
    public Sprite SFurnitureSprite2;
    public Sprite SFurnitureSprite3;

    public Sprite DFurnitureSprite0a;
    public Sprite DFurnitureSprite0b;
    public Sprite DFurnitureSprite1a;
    public Sprite DFurnitureSprite1b;
    public Sprite DFurnitureSprite2a;
    public Sprite DFurnitureSprite2b;
    public Sprite DFurnitureSprite3a;
    public Sprite DFurnitureSprite3b;

    public Sprite TFurnitureSprite0a;
    public Sprite TFurnitureSprite0b;
    public Sprite TFurnitureSprite0c;
    public Sprite TFurnitureSprite1a;
    public Sprite TFurnitureSprite1b;
    public Sprite TFurnitureSprite1c;

    public int AmountOfMines;
    public int AmountofSingles;
    public int AmountofDoubles;
    public int AmountofTriples;

    private int AmountOfSurroundingMines;

    public Raid_Tile[,] grid = new Raid_Tile[9,9];

    public List<Raid_Tile> TilesToCheck = new List<Raid_Tile>();

    [SerializeField, Header("Debug")] private int _randomSeed;
    
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        if (_randomSeed == 0)
        {
            _randomSeed = (int)DateTime.Now.Ticks;
        }
        Debug.Log($"randomSeed {_randomSeed}");
        Random.InitState(_randomSeed);
    }

    private void OnEnable()
    {
        Debug.Log($"");
    }

    private void OnDisable()
    {
        Debug.Log($"");
    }

    private void Start()
    {
        Debug.Log($"");
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

    private IEnumerator RedMarkTiles(int x, int y)
    {
        ColourAdjacentTilesRed(x, y);

        yield return new WaitForSeconds(1);

        ColourAdjacentTilesWhite(x, y);
    }

    public void QuickTapPerformed(Vector2 pointerPosition)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(pointerPosition);

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
                if (raid_Tile.tileType == Raid_Tile.TileType.Number)
                {
                    CalculateAmountOfSurroundingMines(x, y);
                    Raid_Tile NumberTile = grid[x, y];
                    NumberTile.GetComponentInChildren<TextMeshPro>().text = AmountOfSurroundingMines.ToString();
                }
            }
            else if(!raid_Tile.IsCovered && raid_Tile.tileType == Raid_Tile.TileType.Number)
            {
                StartCoroutine(RedMarkTiles(x, y));
            }
        }
        Debug.Log("QuickTap recognized at (" + x + ", " + y + ")");
    }

    public void SlowTapPerformed(Vector2 pointerPosition)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(pointerPosition);

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

    void PlaceSingleTileFurniture()
    {
        int x = Random.Range(0, 9);
        int y = Random.Range(0, 9);

        if (grid[x, y] == null)
        {
            Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Single", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;

            int FurnitureSpriteNumber = Random.Range(0, 4);
            switch(FurnitureSpriteNumber)
            {
                case 0:
                    FurnitureTile.GetComponent<SpriteRenderer>().sprite = SFurnitureSprite0;
                    break;
                case 1:
                    FurnitureTile.GetComponent<SpriteRenderer>().sprite = SFurnitureSprite1;
                    break;
                case 2:
                    FurnitureTile.GetComponent<SpriteRenderer>().sprite = SFurnitureSprite2;
                    break;
                case 3:
                    FurnitureTile.GetComponent<SpriteRenderer>().sprite = SFurnitureSprite3;
                    break;
            }
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
        int x = Random.Range(0, 9);
        int y = Random.Range(0, 9);

        if (grid[x, y] == null)
        {
            if (x+1 < 9 && grid[x+1, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 4);

                switch(FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite0a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite0b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case 1:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite1a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite1b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case 2:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite2a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite2b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case 3:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite3a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite3b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;

                Debug.Log("Double furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Double furniture spawn at (" + (x+1) + ", " + y + ")");

            }
            else if (y+1 < 9 && grid[x, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 4);

                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite0a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite0b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 180);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                    case 1:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite1a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite1b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0,180);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                    case 2:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite2a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite2b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 180);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                    case 3:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite3a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite3b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 180);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;

                Debug.Log("Double furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Double furniture spawn at (" + x + ", " + (y+1) + ")");
            }
            else if (x-1 >= 0 && grid[x-1, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 4);

                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite0a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite0b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case 1:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite1a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite1b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case 2:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite2a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite2b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case 3:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite3a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite3b;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x-1, y] = FurnitureTile2;

                Debug.Log("Double furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Double furniture spawn at (" + (x-1) + ", " + y + ")");
            }
            else if (y-1 >= 0 && grid[x, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Double", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 4);

                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite0a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite0b;
                        break;
                    case 1:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite1a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite1b;
                        break;
                    case 2:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite2a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite2b;
                        break;
                    case 3:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite3a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = DFurnitureSprite3b;
                        break;
                }

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
        int x = Random.Range(0, 9);
        int y = Random.Range(0, 9);

        if (grid[x, y] == null)
        {
            if (x+2 < 9 && grid[x+1, y] == null && grid[x+2, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+2, y, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch(FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0c;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x+2, y] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+2) + ", " + y + ")");
            }
            else if (x+1 < 9 && x-1 >= 0 && grid[x+1, y] == null && grid[x-1, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch(FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0b;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0c;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0a;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x-1, y] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + y + ")");
            }
            else if (x-2 >= 0 && grid[x-1, y] == null && grid[x-2, y] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-2, y, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch(FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0c;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0a;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x-1, y] = FurnitureTile2;
                grid[x-2, y] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-2) + ", " + y + ")");
            }
            else if (y+2 < 9 && grid[x, y+1] == null && grid[x, y+2] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+2, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch(FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0c;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0a;
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;
                grid[x, y+2] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+2) + ")");
            }
            else if (y+1 < 9 && y-1 >= 0 && grid[x, y+1] == null && grid[x, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch(FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0b;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0a;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0c;
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;
                grid[x, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
            }
            else if (y-2 >= 0 && grid[x, y-1] == null && grid[x, y-2] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-2, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch(FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite0c;
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x, y-1] = FurnitureTile2;
                grid[x, y-2] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-2) + ")");
            }
            else if (x+1 < 9 && y+1 < 9 && grid[x+1, y] == null && grid[x+1, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch(FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x+1, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + (y+1) + ")");
            }
            else if (x+1 < 9 && y-1 >= 0 && grid[x+1, y] == null && grid[x+1, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x+1, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + (y-1) + ")");
            }
            else if (x+1 < 9 && y+1 < 9 && grid[x+1, y] == null && grid[x, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
            }
            else if (x+1 < 9 && y-1 >= 0 && grid[x+1, y] == null && grid[x, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 180);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 180);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x+1, y] = FurnitureTile2;
                grid[x, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
            }
            else if (y+1 < 9 && x+1 < 9 && grid[x, y+1] == null && grid[x+1, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 180);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 180);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;
                grid[x+1, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + (y+1) + ")");
            }
            else if (y+1 < 9 && x-1 >= 0 && grid[x, y+1] == null && grid[x-1, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 90);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x, y+1] = FurnitureTile2;
                grid[x-1, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y+1) + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + (y+1) + ")");
            }
            else if (y-1 >= 0 && x+1 < 9 && grid[x, y-1] == null && grid[x+1, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x+1, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x, y-1] = FurnitureTile2;
                grid[x+1, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
                Debug.Log("Triple furniture spawn at (" + (x+1) + ", " + (y-1) + ")");
            }
            else if (y-1 >= 0 && x-1 >= 0 && grid[x, y-1] == null && grid[x-1, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x, y-1] = FurnitureTile2;
                grid[x-1, y-1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + x + ", " + (y-1) + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + (y-1) + ")");
            }
            else if (x-1 >= 0 && y+1 < 9 && grid[x-1, y] == null && grid[x-1, y+1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y+1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, -90);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                }

                grid[x, y] = FurnitureTile;
                grid[x-1, y] = FurnitureTile2;
                grid[x-1, y+1] = FurnitureTile3;

                Debug.Log("Triple furniture spawn at (" + x + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + y + ")");
                Debug.Log("Triple furniture spawn at (" + (x-1) + ", " + (y+1) + ")");
            }
            else if (x-1 >= 0 && y-1 >= 0 && grid[x-1, y] == null && grid[x-1, y-1] == null)
            {
                Raid_Tile FurnitureTile = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile2 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                Raid_Tile FurnitureTile3 = Instantiate(Resources.Load("Prefabs/Triple", typeof(Raid_Tile)), new Vector3(x-1, y-1, 0), Quaternion.identity, _transform) as Raid_Tile;

                int FurnitureSpriteNumber = Random.Range(0, 1);
                switch (FurnitureSpriteNumber)
                {
                    case 0:
                        FurnitureTile.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1c;
                        FurnitureTile2.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1b;
                        FurnitureTile3.GetComponent<SpriteRenderer>().sprite = TFurnitureSprite1a;

                        FurnitureTile.transform.rotation = Quaternion.Euler(0, 0, 180);
                        FurnitureTile2.transform.rotation = Quaternion.Euler(0, 0, 180);
                        FurnitureTile3.transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                }

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
        int x = Random.Range(0, 9);
        int y = Random.Range(0, 9);

        if(grid[x,y] == null)
        {
            Raid_Tile MineTile = Instantiate(Resources.Load("Prefabs/Mine", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;

            grid[x,y] = MineTile;
            Debug.Log("(" + x + ", " + y + ")");
        }
        else
        {
            PlaceMines();
        }
    }

    void CalculateAmountOfSurroundingMines(int x, int y)
    {
        AmountOfSurroundingMines = 0;

        if ((y + 1) < 9 && grid[x, y + 1].tileType == Raid_Tile.TileType.Mine)
        {
            AmountOfSurroundingMines += 1;
        }
        if ((x + 1) < 9 && (y + 1) < 9 && grid[x + 1, y + 1].tileType == Raid_Tile.TileType.Mine)
        {
            AmountOfSurroundingMines += 1;
        }
        if ((x + 1) < 9 && grid[x + 1, y].tileType == Raid_Tile.TileType.Mine)
        {
            AmountOfSurroundingMines += 1;
        }
        if ((x + 1) < 9 && (y - 1) >= 0 && grid[x + 1, y - 1].tileType == Raid_Tile.TileType.Mine)
        {
            AmountOfSurroundingMines += 1;
        }
        if ((y - 1) >= 0 && grid[x, y - 1].tileType == Raid_Tile.TileType.Mine)
        {
            AmountOfSurroundingMines += 1;
        }
        if ((x - 1) >= 0 && (y - 1) >= 0 && grid[x - 1, y - 1].tileType == Raid_Tile.TileType.Mine)
        {
            AmountOfSurroundingMines += 1;
        }
        if ((x - 1) >= 0 && grid[x - 1, y].tileType == Raid_Tile.TileType.Mine)
        {
            AmountOfSurroundingMines += 1;
        }
        if ((x - 1) >= 0 && (y + 1) < 9 && grid[x - 1, y + 1].tileType == Raid_Tile.TileType.Mine)
        {
            AmountOfSurroundingMines += 1;
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
                        Raid_Tile NumberTile = Instantiate(Resources.Load("Prefabs/NumberTile", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;

                        grid[x, y] = NumberTile;
                        NumberTile.GetComponentInChildren<TextMeshPro>().text = ""; // Replace '""' with 'NearbyMines.ToString()' when tiles don't need to be covered.
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
                    Raid_Tile EmptyTile = Instantiate(Resources.Load("Prefabs/Empty_Tile", typeof(Raid_Tile)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Tile;
                    grid[x, y] = EmptyTile;
                }
            }
        }
    }

    public void RevealAdjacentTilesForTileAt (int x, int y)
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

        if ((x-1) >= 0 && (y-1) >= 0)
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
            CalculateAmountOfSurroundingMines(x, y);
            raid_Tile.GetComponentInChildren<TextMeshPro>().text = AmountOfSurroundingMines.ToString();
            Debug.Log("Tile at (" + x + ", " + y + ") is a Number tile");
        }
        else if (raid_Tile.tileType == Raid_Tile.TileType.Furniture)
        {
            Debug.Log("Tile at (" + x + ", " + y + ") is a Furniture tile");
        }
    }

    private void ColourAdjacentTilesRed(int x, int y)
    {
        if ((y + 1) < 9)
        {
            Raid_Tile raid_Tile1 = grid[x, y + 1];
            raid_Tile1.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if ((x + 1) < 9 && (y + 1) < 9)
        {
            Raid_Tile raid_Tile2 = grid[x + 1, y + 1];
            raid_Tile2.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if ((x + 1) < 9)
        {
            Raid_Tile raid_Tile3 = grid[x + 1, y];
            raid_Tile3.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if ((x + 1) < 9 && (y - 1) >= 0)
        {
            Raid_Tile raid_Tile4 = grid[x + 1, y - 1];
            raid_Tile4.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if ((y - 1) >= 0)
        {
            Raid_Tile raid_Tile5 = grid[x, y - 1];
            raid_Tile5.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if ((x - 1) >= 0 && (y - 1) >= 0)
        {
            Raid_Tile raid_Tile6 = grid[x - 1, y - 1];
            raid_Tile6.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if ((x - 1) >= 0)
        {
            Raid_Tile raid_Tile7 = grid[x - 1, y];
            raid_Tile7.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if ((x - 1) >= 0 && (y + 1) < 9)
        {
            Raid_Tile raid_Tile8 = grid[x - 1, y + 1];
            raid_Tile8.GetComponent<SpriteRenderer>().color = Color.red;
        }       
    }

    private void ColourAdjacentTilesWhite(int x, int y)
    {
        if ((y + 1) < 9)
        {
            Raid_Tile raid_Tile1 = grid[x, y + 1];
            raid_Tile1.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if ((x + 1) < 9 && (y + 1) < 9)
        {
            Raid_Tile raid_Tile2 = grid[x + 1, y + 1];
            raid_Tile2.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if ((x + 1) < 9)
        {
            Raid_Tile raid_Tile3 = grid[x + 1, y];
            raid_Tile3.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if ((x + 1) < 9 && (y - 1) >= 0)
        {
            Raid_Tile raid_Tile4 = grid[x + 1, y - 1];
            raid_Tile4.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if ((y - 1) >= 0)
        {
            Raid_Tile raid_Tile5 = grid[x, y - 1];
            raid_Tile5.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if ((x - 1) >= 0 && (y - 1) >= 0)
        {
            Raid_Tile raid_Tile6 = grid[x - 1, y - 1];
            raid_Tile6.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if ((x - 1) >= 0)
        {
            Raid_Tile raid_Tile7 = grid[x - 1, y];
            raid_Tile7.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if ((x - 1) >= 0 && (y + 1) < 9)
        {
            Raid_Tile raid_Tile8 = grid[x - 1, y + 1];
            raid_Tile8.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
