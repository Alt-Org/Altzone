using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Voting;
using MenuUi.Scripts.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopupHandler : MonoBehaviour
{
    [SerializeField] private GameObject Background;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemPriceText;

    [SerializeField] private TextLanguageSelectorCaller _confirmText;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _declineButton;
    [SerializeField] private Button _leftArrowButton;
    [SerializeField] private Button _rightArrowButton;

    [SerializeField] private bool _furnitureShopPermission; //True=shop, false=voteväliaikainen ennen clanroles shop permissionin tarkastus implementaatiota

    private GameFurniture furniture;
    private AvatarPartInfo avatarpart;
    private Sprite _frontSprite;
    private Sprite _sideSprite;
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
        if (Background != null) Background.SetActive(true);
        if (_leftArrowButton != null) _leftArrowButton.gameObject.SetActive(true);
        if (_rightArrowButton != null)_rightArrowButton.gameObject.SetActive(true);

        furniture = newFurniture;
        _itemNameText.text = furniture.Name;
        _itemPriceText.text = furniture.Value.ToString();
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

        if (_furnitureShopPermission)
        {
            _acceptButton.onClick.RemoveAllListeners();
            _acceptButton.onClick.AddListener(() => BuyFurniturePiece());

            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    _confirmText.SetText("Haluatko varmasti ostaa tämän huonekalun?");
                    break;
                case SettingsCarrier.LanguageType.English:
                    _confirmText.SetText("Are you sure you want to buy this item?");
                    break;
                default:
                    _confirmText.SetText("Haluatko varmasti ostaa tämän huonekalun?");
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
                    _confirmText.SetText("Haluatko varmasti aloittaa äänestyksen tästä huonekalusta?");
                    break;
                case SettingsCarrier.LanguageType.English:
                    _confirmText.SetText("Are you sure you want to start a vote for this item?");
                    break;
                default:
                    _confirmText.SetText("Haluatko varmasti aloittaa äänestyksen tästä huonekalusta?");
                    break;
            }
        }//else
    }

    private void SetPopupActiveAvatarPart(AvatarPartInfo part, Sprite icon, string itemName)
    {
        if (Background != null) Background.SetActive(true);
        if (_leftArrowButton != null) _leftArrowButton.gameObject.SetActive(false);
        if (_rightArrowButton != null) _rightArrowButton.gameObject.SetActive(false);

        _itemNameText.text = itemName;
        _itemIcon.sprite = icon;
        avatarpart = part;
        _itemPriceText.text = "100"; //Part.Value.ToString(); (muokkaa ensin GameFurnitureVisualizer:ista
        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(() => BuyAvatarPiece());
        _declineButton.onClick.RemoveAllListeners();
        _declineButton.onClick.AddListener(() => ClosePopup());
        switch (SettingsCarrier.Instance.Language)
        {
            case SettingsCarrier.LanguageType.Finnish:
                _confirmText.SetText("Haluatko varmasti ostaa tämän personointipalan?");
                break;
            case SettingsCarrier.LanguageType.English:
                _confirmText.SetText("Are you sure you want to buy this personification piece?");
                break;
            default:
                _confirmText.SetText("Haluatko varmasti ostaa tämän personointipalan?");
                break;
        }
    }

    public void CreatePollPopup() => StartCoroutine(CreatePollPopupCoroutine());

    //kirpputori äänestys
    public void SetPopupActiveClanStall(StorageFurniture storageFurniture = null)
    {

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
        VotingActions.ReloadPollList?.Invoke();
        ClosePopup();
    }

    private void BuyAvatarPiece()
    {
        //Implement this later.

        ClosePopup();
    }

    private void BuyFurniturePiece() //placeholderi ostomekanismille
    {
        Debug.Log("Ostit juuri huonekalun");
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
    private void ClosePopup()
    {
        _showingFront = true;
        _frontSprite = null;
        _sideSprite = null;
        if (Background != null) Background.SetActive(false);
        if(_leftArrowButton != null) _leftArrowButton.gameObject.SetActive(false);
        if (_rightArrowButton != null) _rightArrowButton.gameObject.SetActive(false);
    }
}
