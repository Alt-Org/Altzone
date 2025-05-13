using UnityEngine;
using UnityEngine.UI;

public class ItemMover : MonoBehaviour
{
    private Transform gridParent;
    private Transform trayParent;

    void Start()
    {

        //Searching Grid and Bar references by name in the koju
        GameObject gridItems = GameObject.Find("PanelContent");
        GameObject trayItems = GameObject.Find("TrayContent");

        if (gridItems != null && trayItems != null)
        {
            gridParent = gridItems.transform;
            trayParent = trayItems.transform;
        }
        else
        {
            Debug.LogError("Error finding Koju grid or tray");
            return;
        }

       GetComponent<Button>().onClick.AddListener(MoveItem);
    }

    void MoveItem()
    {

        //If an item is in the grid, move it to bar. If an item is in the bar, move it to the grid.
        if (transform.parent == gridParent)
        {
            transform.SetParent(trayParent, false);
        }
        else if (transform.parent == trayParent)
        {
            transform.SetParent(gridParent, false);
        }
    }
}
