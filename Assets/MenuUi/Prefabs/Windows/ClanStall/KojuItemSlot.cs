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

        var rectTransform = card.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Set anchors to stretch
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 1f);

            // Reset position and size
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            // Set the pivot just in case
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

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
