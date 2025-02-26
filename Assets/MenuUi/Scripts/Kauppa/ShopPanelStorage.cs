using System;
using System.Collections.ObjectModel;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

/// <summary>
/// Abstract storage component for panels in shop
/// </summary>
public abstract class ShopPanelStorage : MonoBehaviour
{
    // Get all stuff
    // randomixe it
    // (put it to storage)
    // Assign it to objects
    public Action<ReadOnlyCollection<GameFurniture>> getFurnitureAction;
    protected void Start()
    {
        getFurnitureAction += HandleGameFurnitureCreation;
        Storefront.Get().GetAllGameFurnitureYield(getFurnitureAction);
    }
    protected abstract void HandleGameFurnitureCreation(ReadOnlyCollection<GameFurniture> gameFurnitures);
}
