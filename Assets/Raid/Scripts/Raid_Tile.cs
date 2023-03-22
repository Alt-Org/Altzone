using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Raid_Tile : MonoBehaviour
{
    public enum TileType
    {
        Empty, Furniture, Mine, Number
    }

    public enum TileState
    {
        Normal, Flagged
    }

    public enum FurnitureTileSize
    {
        Zero, One, Two, Three
    }

    public bool IsCovered = true;
    public bool DidCheck = false;

    public Sprite CoveredTile;

    public TileType tileType = TileType.Empty;

    public TileState tileState = TileState.Normal;

    public FurnitureTileSize furnitureTileSize = FurnitureTileSize.Zero;

    public Sprite DefaultSprite;

    private void Start()
    {
        DefaultSprite = GetComponent<SpriteRenderer>().sprite;

        GetComponent<SpriteRenderer>().sprite = CoveredTile; // Replace 'CoveredTile' with 'DefaultSprite' when tiles don't need to be covered
    }

    public void SetIsCovered(bool Covered)
    {
        IsCovered = false;
        GetComponent<SpriteRenderer>().sprite = DefaultSprite;
    }
}
