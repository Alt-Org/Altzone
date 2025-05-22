using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KojuPopup : MonoBehaviour
{
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button denyButton;

    [SerializeField] private Button increasePriceButton;
    [SerializeField] private Button decreasePriceButton;

    [SerializeField] private TMP_InputField priceInput;

    [SerializeField] private TMP_Text kojuPriceText;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text setNameText;

    private GameObject currentCard;
    private KojuFurnitureData furnitureData;
    private ItemMover itemMover;

    void Start()
    {
        confirmButton.onClick.AddListener(Confirm);
        denyButton.onClick.AddListener(Deny);

       
        increasePriceButton.onClick.AddListener(OnIncreasePrice);
        decreasePriceButton.onClick.AddListener(OnDecreasePrice);
    }

    //Open the popup and fills it with the necessary data, currently hardcoded for testing purposes
    public void Open(GameObject card)
    {
        currentCard = card;

        furnitureData = currentCard.GetComponent<KojuFurnitureData>();
        itemMover = currentCard.GetComponent<ItemMover>();

        if (furnitureData == null)
        {
            Debug.LogError("Missing KojuFurnitureData component on card.");
            return;
        }

        //Hardcoded test values for now, this data is used in the Popup card. Should use the data directly from the cards in the future.
        nameText.text = "Mirror";
        weightText.text = "10 kg";
        rarityText.text = "Common";
        setNameText.text = "Furniture Set";

        float currentPrice = furnitureData.GetPrice();
        priceInput.text = currentPrice.ToString("F2");
        kojuPriceText.text = currentPrice.ToString("F2");

        gameObject.SetActive(true);
    }

    //Increases price in input field 
    private void OnIncreasePrice()
    {
        if (float.TryParse(priceInput.text, out float price))
        {
            price += 1f;
            priceInput.text = price.ToString("F2");
            UpdateKojuPriceText(price);
        }
    }

    //Decreases price in input field
    private void OnDecreasePrice()
    {
        if (float.TryParse(priceInput.text, out float price))
        {
            price -= 1f;
            if (price < 0) price = 0; //Prevent negative prices
            priceInput.text = price.ToString("F2");
            UpdateKojuPriceText(price);
        }
    }

    private void UpdateKojuPriceText(float price)
    {
        if (kojuPriceText != null)
            kojuPriceText.text = price.ToString("F2");
    }

    //Call when confirming the Popup
    public void Confirm()
    {
        //Assigns the new price
        if (float.TryParse(priceInput.text, out float price))
        {
            furnitureData.SetPrice(price);
        }
        else
        {
            Debug.LogWarning("Invalid price entered");
            return;
        }
        //Moves the item, see ItemMover.cs
        itemMover?.ExecuteMove();

        Close();
    }

    //Ifthe user cancels the Popup
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
