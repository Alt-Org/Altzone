using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Voting;
using MenuUi.Scripts.Storage;
using MenuUi.Scripts.SwipeNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopupHandler : MonoBehaviour
{
    [SerializeField] private GameObject Background;

    [SerializeField] private TextLanguageSelectorCaller _confirmText;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _declineButton;

    private GameFurniture furniture;
    private AvatarPartInfo avatarpart;

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

        furniture = newFurniture;
        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(()=>CreatePollPopup());
        _declineButton.onClick.RemoveAllListeners();
        _declineButton.onClick.AddListener(() => ClosePopup());
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
    }

    private void SetPopupActiveAvatarPart(AvatarPartInfo part)
    {
        if (Background != null) Background.SetActive(true);

        avatarpart = part;
        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(() => BuyAvatarPiece());
        _declineButton.onClick.RemoveAllListeners();
        _declineButton.onClick.AddListener(() => ClosePopup());
        switch (SettingsCarrier.Instance.Language)
        {
            case SettingsCarrier.LanguageType.Finnish:
                _confirmText.SetText("Haluatko varmasti ostaa tämän personointi palasen?");
                break;
            case SettingsCarrier.LanguageType.English:
                _confirmText.SetText("Are you sure you want to buy this personification piece?");
                break;
            default:
                _confirmText.SetText("Haluatko varmasti ostaa tämän personointi palasen?");
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
                _confirmText.SetText("Haluatko varmasti aloittaa äänestyksen tästä huonekalusta?");
                break;
            case SettingsCarrier.LanguageType.English:
                _confirmText.SetText("Are you sure you want to start a vote for this item?");
                break;
            default:
                _confirmText.SetText("Haluatko varmasti aloittaa äänestyksen tästä huonekalusta?");
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

    public IEnumerator CreateClanStallPollPopupCoroutine()
    {
        
        bool? result = null;
               
        if (storageFurnitures != null) PollManager.CreateVotingPoll(FurniturePollType.Buying, storageFurnitures, c => result = c);
        yield return new WaitUntil(() => result.HasValue);
        
        VotingActions.ReloadPollList?.Invoke();
        ClosePopup();
        

    }

    private void ClosePopup()
    {
        if (Background != null) Background.SetActive(false);
    }
}
