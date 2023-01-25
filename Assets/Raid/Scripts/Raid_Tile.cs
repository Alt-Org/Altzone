using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Raid_Tile : MonoBehaviour
{
    public Raid_Grid raid_Grid;
    public enum TileType
    {
        Empty, Furniture, Mine, Number
    }

    public enum TileState
    {
        Normal, Flagged
    }

    public bool IsCovered = true;
    public bool DidCheck = false;

    public Sprite CoveredTile;
    public Sprite FlagTile;

    public TileType tileType = TileType.Empty;

    public TileState tileState = TileState.Normal;

    private Sprite DefaultSprite;

    private void Start()
    {
        raid_Grid = GetComponent<Raid_Grid>();
        DefaultSprite = GetComponent<SpriteRenderer>().sprite;

        GetComponent<SpriteRenderer>().sprite = CoveredTile;
    }

    public void SetIsCovered(bool Covered)
    {
        IsCovered = false;
        GetComponent<SpriteRenderer>().sprite = DefaultSprite;
    }

    public void CheckInputSlowTap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            int x = Mathf.RoundToInt(mousePosition.x);
            int y = Mathf.RoundToInt(mousePosition.y);

            Raid_Tile raid_Tile = raid_Grid.grid[x, y];

            Debug.Log("SlowTap recognized");
            if (raid_Tile.IsCovered)
            {
                if (tileState == TileState.Normal)
                {
                    tileState = TileState.Flagged;
                    GetComponent<SpriteRenderer>().sprite = FlagTile;
                }
                else
                {
                    raid_Tile.tileState = TileState.Normal;
                    GetComponent<SpriteRenderer>().sprite = CoveredTile;
                }
            }
        }
    }
}
