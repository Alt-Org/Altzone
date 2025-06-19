using UnityEngine;
using UnityEngine.UI;

public class ItemMover : MonoBehaviour
{
    private Transform trayParent;
    private Transform gridParent;
    private KojuItemSlot assignedSlot;

    private KojuPopup popup;

    private KojuItemSlot[] panelSlots;

    private KojuTrayPopulator trayPopulator;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetParents(Transform tray, Transform panel)
    {
        trayParent = tray;
        gridParent = panel;

        // Cache KojuItemSlot components in the panel to optimize slot lookup
        panelSlots = gridParent.GetComponentsInChildren<KojuItemSlot>(true);
    }

    public void SetPopup(KojuPopup popupRef)
    {
        popup = popupRef;
    }

    public void SetPopulator(KojuTrayPopulator populator)
    {
        trayPopulator = populator;
    }

    // Call when clicking a card in the panel or the tray, see KojuPopup.cs
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
            trayPopulator?.ShowPanelFullWarning();
        }
    }

    private bool HasFreeSlot()
    {
        foreach (var slot in panelSlots)
        {
            if (!slot.IsOccupied)
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
            foreach (var slot in panelSlots)
            {
                if (!slot.IsOccupied)
                {
                    assignedSlot = slot;
                    assignedSlot.AssignCard(gameObject); // Handles parenting and hiding KojuEmpty
                    transform.SetSiblingIndex(assignedSlot.transform.GetSiblingIndex());
                    return;
                }
            }

            Debug.LogWarning("No available slot found");
        }
        else
        {
            // Moving from panel back to tray
            transform.SetParent(trayParent, false);

            // **Reset price here**
            var furnitureData = GetComponent<KojuFurnitureData>();
            if (furnitureData != null)
            {
                furnitureData.ResetPrice();
            }
            else
            {
                Debug.LogWarning("KojuFurnitureData component not found on item.");
            }

            assignedSlot.ClearSlot(); // Handles showing KojuEmpty again
            assignedSlot = null;
        }
    }
}
