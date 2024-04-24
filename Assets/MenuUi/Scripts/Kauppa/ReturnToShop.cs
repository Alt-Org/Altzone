using UnityEngine;

public class ReturnToShop : MonoBehaviour
{
    public string itemName;
    public int itemPrice;
    public bool isVotingItem = false;
    private bool isBought = false;

    // Funktio, joka asettaa tavaran ‰‰nestett‰v‰ksi
    public void SetAsVotingItem()
    {
        isVotingItem = true;
        Debug.Log(itemName + " has been set as a voting item.");
    }

    // Funktio, joka ‰‰nest‰‰ tavaran puolesta tai vastaan
    public void Vote(bool inFavor)
    {
        if (isVotingItem) 
        {
            if (inFavor)
            {
                Debug.Log(itemName + " has been voted for.");
                // Lis‰‰ t‰h‰n ‰‰nestyksen logiikka
                // Esim. lis‰‰ pisteit‰ tai vastaavaa
            }
            else
            {
                Debug.Log(itemName + " has been voted against.");
                ReturnShop();
            }
        }
        else
        {
            Debug.Log(itemName + " is not a voting item.");
        }
    }

    // Funktio, joka palauttaa tavaran takaisin kauppaan
    private void ReturnShop()
    {
        if (isBought)
        {
            isBought = false;
            Debug.Log(itemName + " has been returned to the shop.");
        }
        else
        {
            Debug.Log(itemName + " is already in the shop.");
        }
    }

    // Funktio, joka ostaa tavaran kaupasta
    public void BuyItem()
    {
        if (isBought)
        {
            Debug.Log(itemName + " is already bought.");
        }
        else
        {
            isBought = true;
            Debug.Log(itemName + " has been bought for " + itemPrice + " coins.");
            // Lis‰‰ t‰h‰n kaupan logiikka
            // Esim. v‰henn‰ pelaajan kolikoita
        }
    }
}
