using UnityEngine;

public class KojuItemSlot : MonoBehaviour
{
    public bool IsOccupied => currentCard != null;
    private GameObject currentCard;

    public void AssignCard(GameObject card, Transform PanelContent)
    {
        //Moves the card to the Koju Panel, disables the available card slot element and replaces that with the assigned card by utilizing the same index.

        currentCard = card;

        
        card.transform.SetParent(PanelContent, false);
        card.transform.SetSiblingIndex(0);

        gameObject.SetActive(false); 
    }

    public void ClearSlot()
    {
        currentCard = null;
        gameObject.SetActive(true); 
    }
}
