using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;

public class GameFurniturePasser : MonoBehaviour
{
    [SerializeField] private Button _acceptButton;

    private string _itemName;
    private Sprite _itemSprite;
    GameFurniture furniture;
    AvatarPartInfo avatarpart;

    public void PassFurnitureToVoting()
    {
        VotingActions.PassShopItem?.Invoke(furniture);
    }

    public void SetGameFurniture(GameFurniture newFurniture)
    {
        furniture = newFurniture;
        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(() => PassFurnitureToVoting());
    }

    public void PassAvatarPart()
    {
        VotingActions.PassShopItemAvatar?.Invoke(avatarpart, _itemSprite, _itemName);
    }

    public void SetAvatarPart(AvatarPartInfo part, Sprite sprite, string itemName)
    {
        _itemName = itemName;
        _itemSprite = sprite;
        avatarpart = part;
        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(() => PassAvatarPart());
    }
}
