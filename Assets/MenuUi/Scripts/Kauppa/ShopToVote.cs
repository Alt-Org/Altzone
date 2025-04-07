using UnityEngine;

public class ShopToVote : MonoBehaviour
{
    public string itemName;
    public int itemPrice;
    public bool isVotingItem = false;

    // Function to buy the item from the shop
    public void BuyItem()
    {
        if (isVotingItem)
        {
            Debug.Log("This item is a voting item and cannot be bought.");
        }
        else
        {
            Debug.Log("Bought " + itemName + " for " + itemPrice + " coins.");
            // Add logic here to deduct coins from player's balance
        }
    }

    // Function to vote for the item
    public void VoteForItem()
    {
        if (!isVotingItem)
        {
            Debug.Log("This item is not a voting item.");
        }
        else
        {
            Debug.Log("Voted for " + itemName);
            // Add logic here to handle voting for the item
        }
    }
    
}
