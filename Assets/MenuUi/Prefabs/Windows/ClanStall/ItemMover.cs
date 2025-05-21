using UnityEngine;
using UnityEngine.UI;

public class ItemMover : MonoBehaviour
{
    private Transform trayParent;
    private Transform gridParent;
    private KojuItemSlot assignedSlot;

    private KojuPopup popup;

    void Start()
    {
        trayParent = GameObject.Find("TrayContent").transform;
        gridParent = GameObject.Find("PanelContent").transform;

        GetComponent<Button>().onClick.AddListener(OnClick);

        //This includes inactive Popup prefab when searching for object of type
        popup = FindObjectOfType<KojuPopup>(true);

        if (popup == null)
        {
            Debug.LogError("KojuPopup not found");
        }
    }

    void OnClick()
    {
        if (assignedSlot != null)
        {
            //If the item is already in the panel, move it immediately to tray
            ExecuteMove();
        }
        else if (HasFreeSlot())
        {
            //Open the popup if the user is trying to move the item from the tray to the panel
            popup?.Open(gameObject);
        }
        else
        {
            Debug.Log("Panel is full!");
        }
    }


    private bool HasFreeSlot()
    {
        return System.Array.Exists(GameObject.FindObjectsOfType<KojuItemSlot>(), slot => !slot.IsOccupied);
    }

    //Called when the user confirms the moving
    public void ExecuteMove()
    {
        //If item is not in grid, try to put it into the grid
        if (assignedSlot == null)
        {
            KojuItemSlot[] slots = GameObject.FindObjectsOfType<KojuItemSlot>();
            foreach (var slot in slots)
            {
                if (!slot.IsOccupied)
                {
                    slot.AssignCard(gameObject, gridParent);
                    assignedSlot = slot;
                    return;
                }
            }

            Debug.Log("No more space available for furniture");
        }
        else
        {
            //If object is already in the grid, put it in the tray
            transform.SetParent(trayParent, false);
            assignedSlot.ClearSlot();
            assignedSlot = null;
        }
    }
}
