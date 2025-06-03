using UnityEngine;

public class KojuItemSlot : MonoBehaviour
{
    [SerializeField] private GameObject kojuEmptyVisual;

    public bool IsOccupied
    {
        get
        {
            foreach (Transform child in transform)
            {
                // Check if the object in the child is an empty visual or not
                if (child.gameObject != kojuEmptyVisual)
                {
                    return true;
                }
            }
            return false;
        }
    }

    // Assign a card to the slot
    public void AssignCard(GameObject card)
    {
        card.transform.SetParent(transform, false);
        if (kojuEmptyVisual != null)
        {
            kojuEmptyVisual.SetActive(false);
        }
    }

    // Clear slot
    public void ClearSlot()
    {
        if (kojuEmptyVisual != null)
        {
            kojuEmptyVisual.SetActive(true);
        }
    }
}
