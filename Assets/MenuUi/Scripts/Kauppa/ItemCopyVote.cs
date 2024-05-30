using UnityEngine;

public class ItemCopyVote : MonoBehaviour
{
    public string itemName;
    public int itemPrice;
    public bool isVotingItem = false;
    private bool isBought = false;

    // Funktio, joka ostaa tavaran kaupasta
    public void BuyItem()
    {
        if (!isBought)
        {
            isBought = true;
            Debug.Log(itemName + " has been bought for " + itemPrice + " coins.");
            // Lis‰‰ t‰h‰n kaupan logiikka
            // Esim. v‰henn‰ pelaajan kolikoita
        }
        else
        {
            Debug.Log(itemName + " is already bought.");
        }
    }

    // Funktio, joka asettaa tavaran ‰‰nestett‰v‰ksi
    public void SetAsVotingItem()
    {
        if (!isBought)
        {
            Debug.Log("Item must be bought before it can be set as a voting item.");
            return;
        }

        if (!isVotingItem)
        {
            isVotingItem = true;
            Debug.Log(itemName + " has been set as a voting item.");
            // Luo kopio tavarakappaleesta ‰‰nestyst‰ varten
            GameObject votingItem = Instantiate(gameObject);
            Item votingItemComponent = votingItem.GetComponent<Item>();
            
            votingItemComponent.isVotingItem = true; // T‰m‰ on ‰‰nestyskappale, ei tavallinen tavara
            votingItem.transform.SetParent(null); // Irroita ‰‰nestyksen tavarakappale scene-hierarkiasta
        }
        else
        {
            Debug.Log(itemName + " is already a voting item.");
        }
    }

    // Funktio, joka ‰‰nest‰‰ tavaran puolesta tai vastaan
    public void Vote(bool inFavor)
    {
        if (isVotingItem)
        {
            if (inFavor)
            {
                Debug.Log(itemName + " has been voted for.");
                // Add your additional logic here
            }
            else
            {
                Debug.Log(itemName + " has been voted against.");
                // Add logic for when the vote is against
            }
        }
        else
        {
            Debug.Log(itemName + " is not a voting item.");
        }
    }

    // Method to set the bought status of the voting item
    public void SetBoughtStatus(bool status)
    {
        isBought = status;
    }
}
