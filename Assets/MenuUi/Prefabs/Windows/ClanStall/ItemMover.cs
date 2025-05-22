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
        //Check if the furniture is in the panel or in the tray
        if (assignedSlot == null)
        {
            KojuItemSlot[] slots = GameObject.FindObjectsOfType<KojuItemSlot>();

            //Sort the panel using sibling index in the hierarchy
            System.Array.Sort(slots, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            //Do a loop for each slot in the panel to find the first one available
            foreach (var slot in slots)
            {
                if (!slot.IsOccupied && slot.gameObject.activeSelf)
                {
                    assignedSlot = slot;
                    assignedSlot.AssignCard(gameObject, gridParent);

                    //Move the card to the same sibling index as the slot
                    int targetIndex = slot.transform.GetSiblingIndex();
                    gameObject.transform.SetSiblingIndex(targetIndex);

                    return;
                }
            }

            Debug.Log("No more space available for furniture");
        }
        else
        {
            //If the furniture is already in the panel, put it back into the tray
            transform.SetParent(trayParent, false);
            assignedSlot.ClearSlot();
            assignedSlot = null;
        }
    }

}
