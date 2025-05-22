using UnityEngine;

public class KojuItemSlot : MonoBehaviour
{
    public bool IsOccupied => currentCard != null;
    private GameObject currentCard;

    public void AssignCard(GameObject card, Transform PanelContent)
    {
        //Moves the card to the Koju Panel, disables the available card slot element 
        currentCard = card;
        card.transform.SetParent(PanelContent, false);
        gameObject.SetActive(false); 
    }


    public void ClearSlot()
    {
        currentCard = null;
        gameObject.SetActive(true); 
    }
}
