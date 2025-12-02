using System;
using System.Collections;
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
    protected virtual void Start()
    {
        getFurnitureAction += HandleGameFurnitureCreation;
        Storefront.Get().GetAllGameFurnitureYield(getFurnitureAction);
    }

    private void OnEnable()
    {
        if(getFurnitureAction != null) Storefront.Get().GetAllGameFurnitureYield(getFurnitureAction);
    }
    protected virtual void HandleGameFurnitureCreation(ReadOnlyCollection<GameFurniture> gameFurnitures)
    {

    }
}
