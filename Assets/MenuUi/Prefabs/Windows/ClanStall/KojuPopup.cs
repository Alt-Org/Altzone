using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KojuPopup : MonoBehaviour
{
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button denyButton;
    [SerializeField] private TMP_InputField priceInput;

    private GameObject currentCard;
    private KojuFurnitureData furnitureData;
    private ItemMover itemMover;

    void Start()
    {
        confirmButton.onClick.AddListener(Confirm);
        denyButton.onClick.AddListener(Deny);
    }

    public void Open(GameObject card)
    {
        currentCard = card;
        furnitureData = currentCard.GetComponent<KojuFurnitureData>();
        itemMover = currentCard.GetComponent<ItemMover>();

        if (furnitureData == null /*|| itemMover == null*/)
        {
            Debug.LogError("Missing KojuFurnitureData component on card.");
            return;
        }

        // Show the current price (hardcoded or set earlier) in the input field
        priceInput.text = furnitureData.GetPrice().ToString();

        gameObject.SetActive(true);
    }

    public void Confirm()
    {
        if (float.TryParse(priceInput.text, out float price))
        {
            furnitureData.SetPrice(price);  // Update the local price and refresh UI
        }
        else
        {
            Debug.LogWarning("Invalid price entered");
            return;
        }

        // Disabled to isolate price logic:
        itemMover.ExecuteMove();

        Close();
    }

    public void Deny()
    {
        Close();
    }

    private void Close()
    {
        currentCard = null;
        furnitureData = null;
        itemMover = null;
        priceInput.text = "";
        gameObject.SetActive(false);
    }
}
