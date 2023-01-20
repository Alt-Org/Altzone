using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_Tile : MonoBehaviour
{
    public enum TileType
    {
        Empty, Furniture, Mine, Number
    }

    public bool IsCovered = true;

    public Sprite CoveredTile;

    public TileType tileType = TileType.Empty;

    private Sprite DefaultSprite;

    private void Start()
    {
       DefaultSprite = GetComponent<SpriteRenderer>().sprite;

        GetComponent<SpriteRenderer>().sprite = CoveredTile;
    }

    public void SetIsCovered(bool Covered)
    {
        IsCovered = false;
        GetComponent<SpriteRenderer>().sprite = DefaultSprite;
    }
}
