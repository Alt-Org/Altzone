using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Store;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdPosterHandler : AltMonoBehaviour
{
    [SerializeField]
    private Image _adBackground;
    [SerializeField]
    private Image _adFrameBorder;
    [SerializeField]
    private Image _adItemImage;
    [SerializeField]
    private ClanHeartColorSetter _adClanLogo;
    [SerializeField]
    private TextMeshProUGUI _adClanName;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetClanData(data =>
        {
            if(data != null) SetAdPoster(data.AdData, data.Name, data.ClanHeartPieces);
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
