using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
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
    private Color? _adColour = null;
    private Sprite _adFrames = null;


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


    // Apply saved colour and border when the object is enabled
    public void OnEnable()
    {
        if (_adColour != null)
        {
            _adBackground.color = _adColour.Value;
        }

        if (_adFrames != null)
        {
            _adFrameBorder.sprite = _adFrames;
        }

    }

    public void RandomizeAdBackgroundColor()
    {
        // If colour is already set, use it and skip randomization
        if (_adColour != null)
        {
            _adBackground.color = _adColour.Value;
            return;
        }

        // Get available colours from the reference list
        var colours = AdDecorationReference.Instance.ColourList;

        
        if (colours != null && colours.Count > 0)
        {
            // Pick a random color from the reference list
            int randomIndex = UnityEngine.Random.Range(0, colours.Count);

            // Save the color to variable
            _adColour = colours[randomIndex];
            _adBackground.color = _adColour.Value;

        }
    }

    public void RandomizeAdFrames()
    {
        // If a border is already set, use it and skip randomization
        if (_adFrames != null)
        {
            _adFrameBorder.sprite = _adFrames;
            return;
        }

        // Get available frames from the reference list
        var frames = AdDecorationReference.Instance.FrameList;

        if (frames != null && frames.Count > 0)
        {
            // Pick a random frame from the reference list
            int randomIndex = UnityEngine.Random.Range(0, frames.Count);
            _adFrames = frames[randomIndex].Image;

            // Save the sprite to variable
            _adFrameBorder.sprite = _adFrames;

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
