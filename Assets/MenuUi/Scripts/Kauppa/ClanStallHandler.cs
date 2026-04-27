using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Storage;
using MenuUI.Scripts.SoulHome;
using TMPro;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class ClanStallPopupHandler : MonoBehaviour
{

    [SerializeField] private GameObject _kojuSlot;
    [SerializeField] private GameObject _kojuCard;
    [SerializeField] private Transform _content;
    private DataStore _store;

    [Header("Information GameObject")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _weight;
    [SerializeField] private TMP_Text _diagnoseNumber;
    [SerializeField] private TMP_Text _artist;
    [SerializeField] private TMP_Text _artisticDescription;
    private List<StorageFurniture> _items;

    [SerializeField] private Button _suggestVotingButton;
    [SerializeField ]private ConfirmationPopupHandler _confirmPopup;

    [SerializeField] private TMP_Text _clanName;
    private string _randomClanName;


    //TO DO: kirpputori äänestys
    void Start()
    {
        //_suggestVotingButton.onClick.AddListener(() => { _confirmPopup.SetPopupActiveClanStall();  });

           
        //showClanName(null);
        
    }

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
       
        _items = storageFurniture;

        for (int i = 0; i < _items.Count; i++)
        {
            
            GameObject slot = Instantiate(_kojuSlot, _content);
            GameObject card = Instantiate(_kojuCard, slot.transform);

         
            card.GetComponent<FurnitureCardUI>().PopulateCard(_items[i]);

            showClanName(null);

            //1. Store the current index so each button remembers its own item
            int itemIndex = i;

            //2. Set up the button to display information of specific item
            Button button = card.GetComponent<Button>();
            if (button != null)
            {
                
                button.onClick.AddListener(() => {
                    showInfo(itemIndex);
                });
            }
        }

        if (_items.Count > 0 && _items != null)
        {
            showInfo(0);
        }

        

    }

    //Populates furniture info in clan stall popup
    void showInfo(int slotVal)
    {
        StorageFurniture _furn = _items[slotVal];

        _suggestVotingButton.onClick.RemoveAllListeners();
        _suggestVotingButton.onClick.AddListener( () => { _confirmPopup.SetPopupActiveClanStall(_furn); });

        //Furniture image
        _icon.sprite = _furn.Sprite;

        //Furniture name
        _name.text = _furn.Info.SetName + " " + _furn.Info.VisibleName;

        //Furniture weight
        _weight.text = _furn.Weight + " KG";

        //Furniture diagnostic number
        _diagnoseNumber.text = _furn.Info.DiagnoseNumber;

        //Furniture designer/artist
        _artist.text = "Suunnittelu: " + _furn.Info.ArtistName;

        //Furniture description
        _artisticDescription.text = _furn.Info.ArtisticDescription;
    }

  
    void randomizeName()
    {
        char[] letters = "qwertyuiopasdfghjklzxcvbnmåöä".ToCharArray();
        System.Random r = new System.Random();

        _randomClanName = "";

        for (int i = 0; i < 8; i++)
        {
            _randomClanName += letters[r.Next(0, letters.Length)];

        }
    }

    void showClanName(ClanData clanData)
    {

        if (clanData == null)
        {
            randomizeName();
            _clanName.text = _randomClanName;
        }
        else
        {
            _clanName.text = clanData.Name;
        }

    }

}
