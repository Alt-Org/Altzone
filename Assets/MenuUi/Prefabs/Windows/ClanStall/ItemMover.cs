using UnityEngine;
using UnityEngine.UI;

public class ItemMover : MonoBehaviour
{
    private Transform trayParent;
    private Transform gridParent;
    private KojuItemSlot assignedSlot;

    void Start()
    {
        //Find containers in the start and add listeners to the buttons in the cards

        trayParent = GameObject.Find("TrayContent").transform;
        gridParent = GameObject.Find("PanelContent").transform;

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        //When clicking the card, move it into an available slot in the Koju panel. If the object is already in the Koju panel, move it back to the tray.

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
            transform.localScale = Vector3.one;
            assignedSlot.ClearSlot();
            assignedSlot = null;
        }
    }
}
