using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;

public class GameFurniturePasser : MonoBehaviour
{
    [SerializeField] private Button _acceptButton;

    GameFurniture furniture;
    AvatarPartsReference.AvatarPartInfo avatarpart;

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
        VotingActions.PassShopItemAvatar?.Invoke(avatarpart);
    }

    public void SetAvatarPart(AvatarPartsReference.AvatarPartInfo part)
    {
        avatarpart = part;
        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(() => PassAvatarPart());
    }
}
