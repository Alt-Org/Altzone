using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Store;
using MenuUi.Scripts.Storage;
using MenuUI.Scripts.SoulHome;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EsineDisplay : AdPosterHandler
{
    
    [SerializeField] private Button _button;

    private List<StorageFurniture> _furnitures;
    private Kirpputori _manager;
    //public TextMeshProUGUI price;


    void Start()
    {
        
        _button.onClick.AddListener(OnAdCLicked);
        
    }

    public void AlignFurnitures(List<StorageFurniture> furnitures, Kirpputori manager)
    {
        _furnitures = furnitures;
        _manager = manager;

        // Displays the first furniture item in the advertisement
        if (furnitures != null && furnitures.Count > 0)
        {
            _adItemImage.sprite = furnitures[0].Sprite;

        }
        
      
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
