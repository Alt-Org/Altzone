using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game;

public class PollInfoPopup : MonoBehaviour
{
    public static PollInfoPopup Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text artistNameText;
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image frontRarityImage;
    [SerializeField] private TMP_Text rarityText;

    [Header("Rarity Color Reference")]
    [SerializeField] private RarityColourReference rarityColourReference;

    [Header("Buttons")]
    [SerializeField] private Button closeInfoButton;

    [Header("Panels")]
    [SerializeField] private GameObject furniturePollInfoObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple PollInfoPopup instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (closeInfoButton != null)
        {
            closeInfoButton.onClick.AddListener(Close);
        }
    }

    public void InitializeIfNeeded()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[PollInfoPopup] Initialized manually.");
        }
    }

    // Opens the popup and fills it with the data from the furniture in question
    public void OpenFurniturePopup(GameFurniture furniture)
    {
        if (furniture == null)
        {
            Debug.LogWarning("PollInfoPopup Open called with null furniture!");
            return;
        }

        nameText.text = furniture.Name ?? "";

        setNameText.text = furniture.FurnitureInfo?.SetName ?? "";
        iconImage.sprite = furniture.FurnitureInfo?.Image;
        descriptionText.text = furniture.FurnitureInfo?.ArtisticDescription ?? "";

        string artistName = furniture.FurnitureInfo?.ArtistName;
        artistNameText.text = string.IsNullOrEmpty(artistName) ? "" : $"Artist: {artistName}";

        weightText.text = $"Weight: {furniture.Weight}";
        valueText.text = $"Value: {furniture.Value}";
        rarityText.text = $"Rarity: {furniture.Rarity}";

        // Apply colour to the two background images of the card based on rarityColourReference
        if (rarityColourReference != null)
        {
            Color rarityColor = rarityColourReference.GetColor(furniture.Rarity);
            rarityImage.color = rarityColor;

            if (frontRarityImage != null)
            {
                frontRarityImage.color = rarityColor;
            }
        }

        gameObject.SetActive(true);
    }

    public void Close()
    {
        furniturePollInfoObject.SetActive(false);
        gameObject.SetActive(false);
    }

    // Toggles the info page in the furniture info popup
    public void ToggleInfo(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(!target.activeSelf);
        }
    }
}
