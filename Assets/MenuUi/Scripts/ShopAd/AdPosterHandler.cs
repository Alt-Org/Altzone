using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Store;
using MenuUi.Scripts.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdPosterHandler : AltMonoBehaviour
{
    [SerializeField]
    protected Image _adBackground;
    [SerializeField]
    protected Image _adFrameBorder;
    [SerializeField]
    protected Image _adItemImage;
    [SerializeField]
    private ClanHeartColorSetter _adClanLogo;
    [SerializeField]
    private TextMeshProUGUI _adClanName;



    // Start is called before the first frame update
    void Start()
    {
        ClanData.OnAdDataUpdated += FetchAdData;
        FetchAdData();
    }

    private void OnEnable()
    {
        FetchAdData();
    }

    private void OnDestroy()
    {
        ClanData.OnAdDataUpdated -= FetchAdData;
    }


    private void FetchAdData()
    {
        StartCoroutine(GetClanData(data =>
        {
            if (data != null)
            {
                SetAdPoster(data.AdData, data.Name, data.ClanHeartPieces);
            }
        }));
    }



    public void SetAdPoster(AdStoreObject data, string clanName, List<HeartPieceData> pieceData)
    {
        _adFrameBorder.sprite = AdDecorationReference.Instance.GetBorderFrameSprite(data.BorderFrame);
        if(_adFrameBorder.sprite) _adFrameBorder.enabled = _adFrameBorder.sprite;
        if (ColorUtility.TryParseHtmlString(data.BackgroundColour, out Color colour)) _adBackground.color = colour;
        _adClanLogo.SetHeartColors(pieceData);
        _adClanName.text = clanName;
    }

    public void SetAdPoster(AdStoreObject data, string clanName)
    {
        _adFrameBorder.sprite = AdDecorationReference.Instance.GetBorderFrameSprite(data.BorderFrame);
        if (_adFrameBorder.sprite) _adFrameBorder.enabled = _adFrameBorder.sprite;
        if (ColorUtility.TryParseHtmlString(data.BackgroundColour, out Color colour)) _adBackground.color = colour;
        _adClanName.text = clanName;
    }


}
