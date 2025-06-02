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
        

        GetComponent<Button>().onClick.AddListener(OnClick);

        // This should be changed
        popup = FindObjectOfType<KojuPopup>(true);

        if (popup == null)
        {
            Debug.LogError("KojuPopup not found");
        }
    }

    
    public void SetParents(Transform tray, Transform panel)
    {
        trayParent = tray;
        gridParent = panel;
    }

    // Call when clicking a card in the panel or the tray, see KojuPopup.cs
    void OnClick()
    {
        if (assignedSlot != null)
        {
            popup?.OpenRemovePopup(gameObject);
        }
        else if (HasFreeSlot())
        {
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

    // Call when user confirms the moving of a furniture
    public void ExecuteMove()
    {
        if (assignedSlot == null)
        {
            KojuItemSlot[] slots = GameObject.FindObjectsOfType<KojuItemSlot>();

            //Sort the panel using sibling index in the hierarchy
            System.Array.Sort(slots, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            // Loop through each slot in the panel to find the first available slot
            foreach (var slot in slots)
            {
                if (!slot.IsOccupied && slot.gameObject.activeSelf)
                {
                    assignedSlot = slot;
                    assignedSlot.AssignCard(gameObject, gridParent);

                    // Move the card to the same sibling index as the slot
                    int targetIndex = slot.transform.GetSiblingIndex();
                    gameObject.transform.SetSiblingIndex(targetIndex);

                    return;
                }
            }

            Debug.Log("No more space available for furniture");
        }
        else // If furniture is already in the panel, put it back into the tray
        {
            transform.SetParent(trayParent, false);
            assignedSlot.ClearSlot();
            assignedSlot = null;
        }
    }
}
