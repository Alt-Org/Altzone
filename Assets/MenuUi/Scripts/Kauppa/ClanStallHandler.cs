using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Storage;
using UnityEngine;

public class ClanStallPopupHandler : MonoBehaviour
{

    [SerializeField] private GameObject _kojuSlot;
    [SerializeField] private GameObject _kojuCard;
    [SerializeField] private Transform _content;
    private DataStore _store;



    private void OnEnable()
    {
        // Populate tray and initialize StoreFront
        _store = Storefront.Get();
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
    
    private void OnDisable()
    {
        for (int i = _content.transform.childCount; i > 0; i--)
        {
            Destroy(_content.transform.GetChild(i - 1).gameObject);
        }
    }
    
    public void CreateStalls(List<StorageFurniture> storageFurniture)
    {
        foreach (StorageFurniture furniture in storageFurniture)
        {
            GameObject slot = Instantiate(_kojuSlot, _content);
            GameObject card = Instantiate(_kojuCard, slot.transform);

            card.GetComponent<FurnitureCardUI>().PopulateCard(furniture);
        }
    }
}
