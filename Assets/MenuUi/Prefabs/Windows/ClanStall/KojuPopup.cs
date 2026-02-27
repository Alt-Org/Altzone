using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game;

public class KojuPopup : MonoBehaviour
{
    [Header("Popup Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button denyButton;
    [SerializeField] private Button increasePriceButton;
    [SerializeField] private Button decreasePriceButton;

    [Header("Price UI")]
    [SerializeField] private TMP_InputField priceInput;
    [SerializeField] private TMP_Text kojuPriceText;

    [Header("Info UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image rarity;
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text artistNameText;

    [Header("Panels")]
    [SerializeField] private GameObject infoObject;
    [SerializeField] private GameObject removePopup;

    [Header("Remove Confirmation")]
    [SerializeField] private Button removeConfirmButton;
    [SerializeField] private Button removeCancelButton;

    [Header("Backgrounds")]
    [SerializeField] private Image infoBoxBackground;
    [SerializeField] private Image removePopupBackground;

    [Header("Rarity Color Reference")]
    [SerializeField] private RarityColourReference rarityColourReference;

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

        removeConfirmButton.onClick.AddListener(ConfirmRemove);
        removeCancelButton.onClick.AddListener(Close);
    }

    // Opens the popup and fills it with the necessary data
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
        weightText.text = cardUI.GetWeightText();
        iconImage.sprite = cardUI.GetIcon();

        // Use stored enum to get color and name
        FurnitureRarity rarityEnum = cardUI.GetFurnitureRarity();
        Color rarityColor = rarityColourReference.GetColor(rarityEnum);
        rarity.color = rarityColor;
        rarityText.text = $"Rarity: {rarityEnum}";

        // Price fields
        priceInput.text = currentPrice.ToString("0.0");
        kojuPriceText.text = $"Value: {currentPrice:0.0}";

        // Description
        descriptionText.text = cardUI.GetDescriptionText();
        artistNameText.text = $"Artist: {cardUI.GetCreatorText()}";

        removePopup.SetActive(false);
        gameObject.SetActive(true);

        // Set background color to match rarity
        if (infoBoxBackground != null)
        {
            infoBoxBackground.color = rarityColor;
        }
    }

    // Opens the popup in removal confirmation mode
    public void OpenRemovePopup(GameObject card)
    {
        currentCard = card;
        itemMover = card.GetComponent<ItemMover>();

        // Apply rarity color to the background in RemovePopup
        var cardUI = card.GetComponent<FurnitureCardUI>();
        if (cardUI != null && removePopupBackground != null)
        {
            Color rarityColor = cardUI.GetRarityColor();
            removePopupBackground.color = rarityColor;
        }

        removePopup.SetActive(true);
        gameObject.SetActive(true);
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

    // Called when confirming the Popup
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

        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");

        // Moves the item, see ItemMover.cs
        itemMover?.ExecuteMove();
        Close();
    }

    // Called when confirming moving from panel to tray
    private void ConfirmRemove()
    {
        itemMover?.ExecuteMove();
        Close();
    }

    // Called when pressing the cancel button
    private void Close()
    {
        currentCard = null;
        furnitureData = null;
        itemMover = null;
        priceInput.text = "";
        infoObject.SetActive(false);
        removePopup.SetActive(false);
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
