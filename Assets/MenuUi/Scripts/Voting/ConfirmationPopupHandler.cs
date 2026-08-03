using System.Collections;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Voting;
using MenuUi.Scripts.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopupHandler : MonoBehaviour
{
    private const float AvatarPartPrice = 1.99f;
    private const string EuroSuffix = " €";

    [SerializeField] private GameObject Background;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemPriceText;
    [SerializeField] private GameObject _coinImage;
    [SerializeField] private Image _popupBackgroundImage;
    [SerializeField] private VerticalGradientEffect _popupBackgroundGradient;
    [SerializeField] private Image _popupTabImage;

    [SerializeField] private TextLanguageSelectorCaller _confirmText;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _declineButton;
    [SerializeField] private Button _leftArrowButton;
    [SerializeField] private Button _rightArrowButton;
    [SerializeField] private Color _personificationShopPopupColor = new Color(0.188f, 0.804f, 0.325f, 0.93f);
    [SerializeField] private Color _furnitureShopPopupColor = new Color(0.639f, 0.839f, 0.941f, 0.93f);

    private GameFurniture furniture;
    private Sprite _frontSprite;
    private Sprite _sideSprite;
    private string _avatarItemName;
    private bool _showingFront = true;

    private StorageFurniture storageFurnitures;

    private void OnEnable()
    {
        VotingActions.PassShopItem += SetPopupActive;
        VotingActions.PassShopItemAvatar += SetPopupActiveAvatarPart;
    }

    private void OnDisable()
    {
        VotingActions.PassShopItem -= SetPopupActive;
        VotingActions.PassShopItemAvatar -= SetPopupActiveAvatarPart;
    }

    private void SetPopupActive(GameFurniture newFurniture)
    {
        ApplyPopupStyle(_furnitureShopPopupColor);

        if (Background != null) Background.SetActive(true);
        if (_leftArrowButton != null) _leftArrowButton.gameObject.SetActive(true);
        if (_rightArrowButton != null)_rightArrowButton.gameObject.SetActive(true);

        furniture = newFurniture;
        _itemNameText.text = furniture.Name;
        _itemPriceText.text = furniture.Value.ToString();
        SetCoinImageVisible(true);
        _itemIcon.sprite = furniture.FurnitureInfo.Image;
        _frontSprite = furniture.FurnitureInfo.Image;
        _sideSprite = furniture.FurnitureInfo.SidewaysImage;
        _showingFront = true;

        _leftArrowButton.onClick.RemoveAllListeners();
        _leftArrowButton.onClick.AddListener(() => TogglePopupIcon());
        _rightArrowButton.onClick.RemoveAllListeners();
        _rightArrowButton.onClick.AddListener(() => TogglePopupIcon());
        _declineButton.onClick.RemoveAllListeners();
        _declineButton.onClick.AddListener(() => ClosePopup());

        if (HasFurnitureShopPermission())
        {
            _acceptButton.onClick.RemoveAllListeners();
            _acceptButton.onClick.AddListener(() => BuyFurniturePiece());

            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    _confirmText.SetText("Haluatko ostaa tämän huonekalun?");
                    break;
                case SettingsCarrier.LanguageType.English:
                    _confirmText.SetText("Are you sure you want to buy this item?");
                    break;
                default:
                    _confirmText.SetText("Haluatko ostaa tämän huonekalun?");
                    break;
            }
        }
        else
        {
            _acceptButton.onClick.RemoveAllListeners();
            _acceptButton.onClick.AddListener(() => CreatePollPopup());

            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    _confirmText.SetText("Haluatko äänestää tästä huonekalusta?");
                    break;
                case SettingsCarrier.LanguageType.English:
                    _confirmText.SetText("Do you want to vote for this item?");
                    break;
                default:
                    _confirmText.SetText("Haluatko äänestää tästä huonekalusta?");
                    break;
            }
        }//else
    }

    private static bool HasFurnitureShopPermission()
    {
        ServerManager serverManager = ServerManager.Instance;
        if (serverManager == null || serverManager.Player == null || serverManager.Clan == null)
            return false;

        if (serverManager.Player.clan_id != serverManager.Clan._id)
            return false;

        string roleId = serverManager.Player.clanRole_id;
        if (string.IsNullOrEmpty(roleId))
            return false;

        ClanRoles role = serverManager.Clan.roles?.Find(candidate => candidate._id == roleId);
        return role?.rights?.shop == true;
    }

    private void SetPopupActiveAvatarPart(AvatarPartInfo part)
    {
        ApplyPopupStyle(_personificationShopPopupColor);

        if (Background != null) Background.SetActive(true);
        if (_leftArrowButton != null) _leftArrowButton.gameObject.SetActive(false);
        if (_rightArrowButton != null) _rightArrowButton.gameObject.SetActive(false);

        string itemName = AvatarPartDisplay.GetName(part);
        _itemNameText.text = itemName;
        _itemIcon.sprite = AvatarPartDisplay.GetImage(part);
        _avatarItemName = itemName;
        _itemPriceText.text = FormatEuroPrice(AvatarPartPrice);
        SetCoinImageVisible(false);
        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(() => BuyAvatarPiece());
        _declineButton.onClick.RemoveAllListeners();
        _declineButton.onClick.AddListener(() => ClosePopup());
        switch (SettingsCarrier.Instance.Language)
        {
            case SettingsCarrier.LanguageType.Finnish:
                _confirmText.SetText("Haluatko ostaa tämän personointipalan?");
                break;
            case SettingsCarrier.LanguageType.English:
                _confirmText.SetText("Do you want to buy this personification piece?");
                break;
            default:
                _confirmText.SetText("Haluatko ostaa tämän personointipalan?");
                break;
        }
    }

    public void CreatePollPopup() => StartCoroutine(CreatePollPopupCoroutine());

    //kirpputori äänestys
    public void SetPopupActiveClanStall(StorageFurniture storageFurniture = null)
    {
        ApplyPopupStyle(_furnitureShopPopupColor);

        if (Background != null) Background.SetActive(true);

        storageFurnitures = storageFurniture;

        transform.SetAsLastSibling();

       
        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(() => CreateClanStallPollPopup());
        _declineButton.onClick.RemoveAllListeners();
        _declineButton.onClick.AddListener(() => ClosePopup());

        

        switch (SettingsCarrier.Instance.Language)
        {
            case SettingsCarrier.LanguageType.Finnish:
                _confirmText.SetText("Haluatko varmasti aloittaa äänestyksen tästä huonekalusta?\n\nHuom. Tämä ei tällä hetkellä luo vielä oikeaa äänestystä.");
                break;
            case SettingsCarrier.LanguageType.English:
                _confirmText.SetText("Are you sure you want to start a vote for this item?\n\nNote. This doesn't yet start an actual proper vote.");
                break;
            default:
                _confirmText.SetText("Haluatko varmasti aloittaa äänestyksen tästä huonekalusta?\n\nHuom. Tämä ei tällä hetkellä luo vielä oikeaa äänestystä.");
                break;
        }


    }

    public void CreateClanStallPollPopup() => StartCoroutine(CreateClanStallPollPopupCoroutine());

    public IEnumerator CreatePollPopupCoroutine()
    {
        bool? result = null;
        if (furniture != null) PollManager.CreateShopFurniturePoll(FurniturePollType.Buying, furniture, c => result = c);
        yield return new WaitUntil(()=> result.HasValue);
        if (result == true && furniture != null)
            VotingActions.ShopItemInVoting?.Invoke(furniture.Name);
        VotingActions.ReloadPollList?.Invoke();
        ClosePopup();
    }

    private void BuyAvatarPiece()
    {
        ClosePopup();
        string boughtItemName = _avatarItemName;
        VotingActions.RequestPlayTransaction(() => VotingActions.AvatarShopItemBought?.Invoke(boughtItemName));
    }

    private void BuyFurniturePiece() => StartCoroutine(BuyFurniturePieceCoroutine());

    private IEnumerator BuyFurniturePieceCoroutine()
    {
        if (furniture == null)
            yield break;

        string boughtItemName = furniture.Name;
        bool? result = null;
        PollManager.CreateShopFurniturePoll(FurniturePollType.Buying, furniture, c => result = c);
        yield return new WaitUntil(() => result.HasValue);

        if (result == true)
            VotingActions.ShopItemBought?.Invoke(boughtItemName);

        ClosePopup();
    }

    public IEnumerator CreateClanStallPollPopupCoroutine()
    {
        
        bool? result = null;
               
        if (storageFurnitures != null) PollManager.CreateVotingPoll(FurniturePollType.Buying, storageFurnitures, c => result = c);
        yield return new WaitUntil(() => result.HasValue);
        
        VotingActions.ReloadPollList?.Invoke();
        ClosePopup();
        

    }

    private void TogglePopupIcon() //huonekalujen kääntäminen popupissa
    {
        if (_showingFront && _sideSprite == null) return;
        _showingFront = !_showingFront;
        _itemIcon.sprite = _showingFront ? _frontSprite : _sideSprite;
    }
    private void ApplyPopupStyle(Color accentColor)
    {
        if (_popupBackgroundGradient != null)
        {
            _popupBackgroundGradient.SetGradient(Color.white, accentColor);

            if (_popupBackgroundImage != null)
                _popupBackgroundImage.color = Color.white;
        }
        else if (_popupBackgroundImage != null)
        {
            _popupBackgroundImage.color = accentColor;
        }

        if (_popupTabImage == null)
            return;

        Color tabColor = accentColor;
        tabColor.a = 1f;
        _popupTabImage.color = tabColor;
    }

    private void ClosePopup()
    {
        _showingFront = true;
        _frontSprite = null;
        _sideSprite = null;
        if (Background != null) Background.SetActive(false);
        if(_leftArrowButton != null) _leftArrowButton.gameObject.SetActive(false);
        if (_rightArrowButton != null) _rightArrowButton.gameObject.SetActive(false);
    }

    private static string FormatEuroPrice(float value)
    {
        return $"{value:0.##}{EuroSuffix}";
    }

    private void SetCoinImageVisible(bool isVisible)
    {
        if (_coinImage != null)
            _coinImage.SetActive(isVisible);
    }
}
