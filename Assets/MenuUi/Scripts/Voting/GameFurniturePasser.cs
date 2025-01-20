using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public class GameFurniturePasser : MonoBehaviour
{
    GameFurniture furniture;

    public void PassFurnitureToVoting()
    {
        VotingActions.PassShopItem?.Invoke(furniture);
    }

    public void SetGameFurniture(GameFurniture newFurniture)
    {
        furniture = newFurniture;
    }
}
