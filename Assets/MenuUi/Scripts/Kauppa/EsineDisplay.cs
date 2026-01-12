using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Storage;
using MenuUI.Scripts.SoulHome;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EsineDisplay : MonoBehaviour
{
    public KauppaItems items;
    public TextMeshProUGUI price;
    public Image contentImage;

    private List<StorageFurniture> _furnitures;
    private Kirpputori _manager;

    void Start()
    {
        price.text = items.hinta;
        
        contentImage.sprite = items.esine;
    }

    public void AlignFurnitures(List<StorageFurniture> furnitures, Kirpputori manager)
    {
        _furnitures = furnitures;
        _manager = manager;
    }

    public void OnAdCLicked()
    {
        _manager.OpenPopup(_furnitures);
    }

    public void PassItemToVoting()
    {
        //VotingActions.PassKauppaItem?.Invoke(this);
    }

}
