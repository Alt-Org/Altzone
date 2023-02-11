using UnityEngine;
using UnityEngine.InputSystem;

public class Raid_Input : MonoBehaviour
{
    public delegate void QuickTapped();
    public event QuickTapped OnQuickTapped;

    public delegate void SlowTapped();
    public event SlowTapped OnSlowTapped;

    private RaidControls raidControls;
    private Raid_Grid raid_Grid;

    public Sprite CoveredTile { get; private set; }
    public Sprite FlagTile { get; private set; }

    private void Awake()
    {
        raidControls = new RaidControls();
        raid_Grid = new Raid_Grid();
    }

    private void OnEnable()
    {
        raidControls.Enable();
    }
    private void OnDisable()
    {
        raidControls.Disable();
    }

    private void Start()
    {
        raidControls.Touch.Tap.performed += ctx => raid_Grid.QuickTapPerformed(ctx);
        raidControls.Touch.SlowTap.performed += ctx => raid_Grid.SlowTapPerformed(ctx);
    }

    public void QuickTapPerformed(InputAction.CallbackContext context)
    {
        if (OnQuickTapped != null) OnQuickTapped();
        {
            Vector2 pos = ScreenPosition();
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.y);

            Raid_Tile raid_Tile = raid_Grid.grid[x, y];

            if (raid_Tile.tileState == Raid_Tile.TileState.Normal)
            {
                if (raid_Tile.IsCovered)
                {
                    raid_Tile.SetIsCovered(false);

                    if (raid_Tile.tileType == Raid_Tile.TileType.Empty)
                    {
                        raid_Grid.RevealAdjacentTilesForTileAt(x, y);
                    }
                }
            }
            Debug.Log("QuickTap recognized at (" + x + ", " + y + ")");
        }
    }

    public void SlowTapPerformed(InputAction.CallbackContext context)
    {
        if (OnSlowTapped != null) OnSlowTapped();
        {
            Vector2 pos = ScreenPosition();
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.y);

            Raid_Tile raid_Tile = raid_Grid.grid[x, y];
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

    private Vector2 ScreenPosition() => raidControls.Touch.TapPosition.ReadValue<Vector2>();
}
