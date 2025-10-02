using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using Altzone.Scripts.Voting;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ConfirmationPopupHandler : MonoBehaviour
{
    [SerializeField] private GameObject Background;

    [SerializeField] private TextMeshProUGUI _confirmText;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _declineButton;

    private GameFurniture furniture;
    private AvatarPartsReference.AvatarPartInfo avatarpart;

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
        _confirmText.text = "Haluatko varmasti aloittaa äänestyksen tästä huonekalusta?";
    }

    private void SetPopupActiveAvatarPart(AvatarPartsReference.AvatarPartInfo part)
    {
        if (Background != null) Background.SetActive(true);

        avatarpart = part;
        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(() => BuyAvatarPiece());
        _declineButton.onClick.RemoveAllListeners();
        _declineButton.onClick.AddListener(() => ClosePopup());
        _confirmText.text = "Haluatko varmasti ostaa tämän personointi palasen?";
    }

    public void CreatePollPopup()
    {
        StartCoroutine(CreatePollPopup(furniture));
    }

    public IEnumerator CreatePollPopup(GameFurniture furniture)
    {
        if (furniture != null)
        {
            bool? pollCreated = null;
            StartCoroutine(ServerManager.Instance.BuyItemToClanFromServer(furniture.Name, value=> pollCreated= value));
            yield return new WaitUntil(() => pollCreated != null);
            PollManager.CreateFurniturePoll(FurniturePollType.Buying, furniture); 
            VotingActions.ReloadPollList?.Invoke();

            FindObjectOfType<SwipeUI>(true).CurrentPage = 3;
        }

        ClosePopup();
    }

    private void BuyAvatarPiece()
    {
        //Implement this later.

        ClosePopup();
    }

    private void ClosePopup()
    {
        if (Background != null) Background.SetActive(false);
    }
}
