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
    [SerializeField] private Image rarity;
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private Image iconImage;

    private GameObject currentCard;
    private KojuFurnitureData furnitureData;
    private ItemMover itemMover;

    void Start()
    {
        confirmButton.onClick.AddListener(Confirm);
        denyButton.onClick.AddListener(Close);
        increasePriceButton.onClick.AddListener(OnIncreasePrice);
        decreasePriceButton.onClick.AddListener(OnDecreasePrice);
        priceInput.onValueChanged.AddListener(OnPriceInputChanged);
        priceInput.characterLimit = 4;
    }

    //Opens the popup and fills it with the necessary data
    public void Open(GameObject card)
    {
        currentCard = card;
        var cardUI = card.GetComponent<FurnitureCardUI>();
        itemMover = card.GetComponent<ItemMover>();

        if (cardUI == null)
        {
            Debug.LogError("Missing FurnitureCardUI component on the card.");
            return;
        }

        furnitureData = card.GetComponent<KojuFurnitureData>();
        float currentPrice = furnitureData != null ? furnitureData.GetPrice() : 0;

        nameText.text = cardUI.GetNameText();
        setNameText.text = cardUI.GetSetNameText();
        rarity.color = cardUI.GetRarityColor();
        weightText.text = cardUI.GetWeightText();
        iconImage.sprite = cardUI.GetIcon();

        rarityText.text = "Rarity: " + GetRarityNameFromColor(rarity.color);

        priceInput.text = currentPrice.ToString("0.0");              
        kojuPriceText.text = $"Value: {currentPrice:0.0}";         

        gameObject.SetActive(true);
    }

    //Gets the rarity text for the card based on the retrieved color from data
    private string GetRarityNameFromColor(Color color)
    {
        if (color == Color.gray) return "Common";
        if (color == Color.cyan) return "Rare";
        if (color == new Color(0.6f, 0.2f, 0.8f)) return "Epic";
        if (color == new Color(1f, 0.55f, 0f)) return "Antique";
        return "Unknown";
    }

 
    private void OnIncreasePrice()
    {
        if (float.TryParse(priceInput.text, out float price))
        {
            price += 1f;
            priceInput.text = price.ToString("0.0");
            UpdateKojuPriceText(price);
        }
    }

   
    private void OnDecreasePrice()
    {
        if (float.TryParse(priceInput.text, out float price))
        {
            price = Mathf.Max(0, price - 1f);
            priceInput.text = price.ToString("0.0");
            UpdateKojuPriceText(price);
        }
    }

    private void UpdateKojuPriceText(float price)
    {
        if (kojuPriceText != null)
            kojuPriceText.text = $"Value: {price:0.0}";
    }

    private void OnPriceInputChanged(string input)
    {
        if (float.TryParse(input, out float price))
        {
            UpdateKojuPriceText(price);
        }
    }

    //Called when confirming the Poup
    public void Confirm()
    {
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

    //Called when pressing the deny button
    private void Close()
    {
        currentCard = null;
        furnitureData = null;
        itemMover = null;
        priceInput.text = "";
        gameObject.SetActive(false);
    }

    public void ToggleInfo(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(!target.activeSelf);
        }
    }
}
