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
    }

    public void SetParents(Transform tray, Transform panel)
    {
        trayParent = tray;
        gridParent = panel;
    }

    public void SetPopup(KojuPopup popupRef)
    {
        popup = popupRef;
    }

    //Call when clicking a card in the panel or the tray, see KojuPopup.cs
    private void OnClick()
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
        foreach (Transform child in gridParent)
        {
            var slot = child.GetComponent<KojuItemSlot>();
            if (slot != null && !slot.IsOccupied)
            {
                return true;
            }
        }

        return false;
    }

    // Call when user confirms the moving of a furniture
    public void ExecuteMove()
    {
        if (assignedSlot == null)
        {
            // Moving from tray to panel
            foreach (Transform child in gridParent)
            {
                var slot = child.GetComponent<KojuItemSlot>();
                if (slot != null && !slot.IsOccupied)
                {
                    assignedSlot = slot;
                    assignedSlot.AssignCard(gameObject); // Handles parenting and hiding KojuEmpty
                    transform.SetSiblingIndex(child.GetSiblingIndex());
                    return;
                }
            }

            Debug.LogWarning("No available slot found, but HasFreeSlot returned true. Check slot logic.");
        }
        else
        {
            // Moving from panel back to tray
            transform.SetParent(trayParent, false);
            assignedSlot.ClearSlot(); // Handles showing KojuEmpty again
            assignedSlot = null;
        }
    }
}
