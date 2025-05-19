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

        //This includes inactive Popup when searching for it
        popup = FindObjectOfType<KojuPopup>(true);

        if (popup == null)
        {
            Debug.LogError("KojuPopup not found");
        }
    }

    void OnClick()
    {
        if (assignedSlot != null || HasFreeSlot())
            popup?.Open(gameObject);
        else
            Debug.Log("Panel is full!");
    }

    private bool HasFreeSlot()
    {
        return System.Array.Exists(GameObject.FindObjectsOfType<KojuItemSlot>(), slot => !slot.IsOccupied);
    }

    //Called when the user confirms the moving
    public void ExecuteMove()
    {
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
            transform.SetParent(trayParent, false);
            assignedSlot.ClearSlot();
            assignedSlot = null;
        }
    }
}
