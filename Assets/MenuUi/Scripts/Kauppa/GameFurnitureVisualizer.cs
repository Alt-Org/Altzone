using System;
using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using MenuUI.Scripts;
using Altzone.Scripts.AvatarPartsInfo;
public class GameFurnitureVisualizer : MonoBehaviour
{
    [SerializeField] private Image _contentImage;
    [SerializeField] private TMP_Text _themeText;
    [SerializeField] private TMP_Text _productText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Button _button;
    [SerializeField] private Image _coinImage;
    [SerializeField] private GameObject _priceObject;
    [SerializeField] private GameObject _buyButtonObject;
    [SerializeField] private GameObject _boughtBottomObject;
    [SerializeField] private GameObject _votingBottomObject;
    [SerializeField] private GameObject _votingIconObject;
    private const string BoughtText = "Ostettu";
    private const float AvatarPartPrice = 1.99f;
    private const string EuroSuffix = "€";
    private GameFurniture _gameFurniture;
    private AvatarPartInfo _avatarPart;
    private string _avatarItemName;

    private enum ShopItemState
    {
        Available,
        Bought,
        InVoting
    }

    private void OnEnable()
    {
        VotingActions.ShopItemBought += HandleShopItemBought;
        VotingActions.ShopItemInVoting += HandleShopItemInVoting;
        VotingActions.AvatarShopItemBought += HandleAvatarShopItemBought;
    }

    private void OnDisable()
    {
        VotingActions.ShopItemBought -= HandleShopItemBought;
        VotingActions.ShopItemInVoting -= HandleShopItemInVoting;
        VotingActions.AvatarShopItemBought -= HandleAvatarShopItemBought;
    }

    public void Initialize(GameFurniture gameFurniture, GameObject confirmationPopUp)
    {
        _gameFurniture = gameFurniture;
        _avatarPart = null;
        SetFurnitureDisplayName(_gameFurniture);
        _priceText.text = _gameFurniture.Value.ToString();
        SetState(ShopItemState.Available);
        SetCoinImageVisible(true);
        _contentImage.sprite = _gameFurniture.FurnitureInfo.RibbonImage? _gameFurniture.FurnitureInfo.RibbonImage : _gameFurniture.FurnitureInfo.Image;
        gameObject.GetComponent<GameFurniturePasser>().SetGameFurniture(gameFurniture);
        _button.onClick.AddListener(() => confirmationPopUp.SetActive(true));
        _button.onClick.AddListener(() => gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1"));
    }

    public void Initialize(AvatarPartInfo avatarPart, GameObject confirmationPopUp)
    {
        _gameFurniture = null;
        _avatarPart = avatarPart;
        _avatarItemName = GetAvatarDisplayName(_avatarPart);
        if (_themeText != null)
            _themeText.text = string.Empty;
        _productText.text = _avatarItemName;
        _priceText.text = FormatEuroPrice(AvatarPartPrice); //_avatarPart.Value.ToString();
        SetState(ShopItemState.Available);
        SetCoinImageVisible(false);
        _contentImage.sprite = _avatarPart.IconImage ? _avatarPart.IconImage : _avatarPart.AvatarImage;
        gameObject.GetComponent<GameFurniturePasser>().SetAvatarPart(_avatarPart, _contentImage.sprite, _avatarItemName);
        _button.onClick.AddListener(() => confirmationPopUp.SetActive(true));
        _button.onClick.AddListener(() => gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1"));
    }

    private void HandleShopItemBought(string itemName)
    {
        if (_gameFurniture == null || _gameFurniture.Name != itemName)
            return;

        SetState(ShopItemState.Bought);
    }

    private void HandleAvatarShopItemBought(string itemName)
    {
        if (_avatarPart == null || _avatarItemName != itemName)
            return;

        SetState(ShopItemState.Bought);
    }

    private void HandleShopItemInVoting(string itemName)
    {
        if (_gameFurniture == null || _gameFurniture.Name != itemName)
            return;

        SetState(ShopItemState.InVoting);
    }

    private void SetState(ShopItemState state)
    {
        bool isAvailable = state == ShopItemState.Available;
        bool isBought = state == ShopItemState.Bought;
        bool isInVoting = state == ShopItemState.InVoting;

        if (isBought && _priceText != null)
            _priceText.text = BoughtText;

        if (_boughtBottomObject != null)
            _boughtBottomObject.SetActive(isBought);

        if (_votingBottomObject != null)
            _votingBottomObject.SetActive(isInVoting);

        if (_votingIconObject != null)
            _votingIconObject.SetActive(isInVoting);

        if (_priceObject != null)
            _priceObject.SetActive(isAvailable);

        if (_buyButtonObject != null)
            _buyButtonObject.SetActive(isAvailable);

        if (_button != null)
            _button.interactable = isAvailable;
    }

    private void SetCoinImageVisible(bool isVisible)
    {
        if (_coinImage != null)
            _coinImage.gameObject.SetActive(isVisible);
    }

    private static string FormatEuroPrice(float value)
    {
        return $"{value:0.##}{EuroSuffix}";
    }

    private static string GetAvatarDisplayName(AvatarPartInfo avatarPart)
    {
        if (avatarPart == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(avatarPart.Name))
            return CleanAvatarDisplayName(avatarPart.Name);

        if (!string.IsNullOrWhiteSpace(avatarPart.VisibleName))
            return CleanAvatarDisplayName(avatarPart.VisibleName);

        return CleanAvatarDisplayName(avatarPart.name);
    }

    private static string CleanAvatarDisplayName(string name)
    {
        const string Prefix = "Avatar";
        const string Suffix = "Default";

        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        string displayName = name.Trim();

        if (displayName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            displayName = displayName.Substring(Prefix.Length);

        displayName = displayName.Trim(' ', '_', '-');

        if (displayName.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase))
            displayName = displayName.Substring(0, displayName.Length - Suffix.Length);

        return displayName.Trim(' ', '_', '-');
    }

    private void SetFurnitureDisplayName(GameFurniture gameFurniture)
    {
        (string itemName, string themeName) = GetFurnitureDisplayNameParts(gameFurniture);

        if (_themeText != null)
            _themeText.text = EscapeRichText(themeName);

        if (_productText != null)
            _productText.text = EscapeRichText(itemName);
    }

    private static (string itemName, string themeName) GetFurnitureDisplayNameParts(GameFurniture gameFurniture)
    {
        if (gameFurniture == null)
            return (string.Empty, string.Empty);

        string itemName = gameFurniture.FurnitureInfo?.VisibleName;
        string themeName = gameFurniture.FurnitureInfo?.SetName;

        if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(themeName))
        {
            (string parsedItemName, string parsedThemeName) = ParseFurnitureName(gameFurniture.Name);
            if (string.IsNullOrWhiteSpace(itemName))
                itemName = parsedItemName;

            if (string.IsNullOrWhiteSpace(themeName))
                themeName = parsedThemeName;
        }

        return (itemName, themeName);
    }

    private static (string itemName, string themeName) ParseFurnitureName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (string.Empty, string.Empty);

        string[] parts = name.Split('_');
        if (parts.Length != 2)
            return (name, string.Empty);

        return (parts[0], parts[1]);
    }

    private static string EscapeRichText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
