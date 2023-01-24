using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Raid_Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
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
        DefaultSprite = GetComponent<SpriteRenderer>().sprite;

        GetComponent<SpriteRenderer>().sprite = CoveredTile;
    }

    public void SetIsCovered(bool Covered)
    {
        IsCovered = false;
        GetComponent<SpriteRenderer>().sprite = DefaultSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (IsCovered)
            {
                if (tileState == TileState.Normal)
                {
                    tileState = TileState.Flagged;
                    GetComponent<SpriteRenderer>().sprite = FlagTile;
                }
                else
                {
                    tileState = TileState.Normal;
                    GetComponent<SpriteRenderer>().sprite = CoveredTile;
                }
            }
        }
    }
}
