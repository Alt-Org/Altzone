using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.Window;
using UnityEngine;

public class ClanHeartColorSetter : MonoBehaviour
{
    [SerializeField] private Transform _heartContainer;
    [SerializeField] private bool _setOwnClanHeart = true;
    private HeartPieceColorHandler[] _heartPieceHandlers = { };

    public bool SetOwnClanHeart { set => _setOwnClanHeart = value; }

    private void OnEnable()
    {
        SetClanLogoData(null);
        ServerManager.OnClanChanged += SetClanLogoData;
    }

    private void OnDisable()
    {
        ServerManager.OnClanChanged -= SetClanLogoData;
    }

    private void SetClanLogoData(ServerClan clan)
    {
        if (!_setOwnClanHeart || ServerManager.Instance.Clan == null) return;

        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
        {
            clanData.ClanHeartPieces ??= new();
            List<HeartPieceData> heartPieces = clanData.ClanHeartPieces;

            if (heartPieces.Count == 0)
            {
                for (int j = 0; j < 50; j++) heartPieces.Add(new HeartPieceData(j, Color.white));
            }
            SetHeartColors(heartPieces);
        });
    }

    public void SetOtherClanColors(ClanData clanData)
    {
        clanData.ClanHeartPieces ??= new();
        List<HeartPieceData> heartPieces = clanData.ClanHeartPieces;

        if (heartPieces.Count == 0)
        {
            for (int j = 0; j < 50; j++) heartPieces.Add(new HeartPieceData(j, Color.white));
        }
        SetHeartColors(heartPieces);
    }

    public void SetHeartColors(List<HeartPieceData> heartPieces)
    {
        _heartPieceHandlers = _heartContainer.GetComponentsInChildren<HeartPieceColorHandler>();

        int i = 0;
        foreach (HeartPieceColorHandler colorhandler in _heartPieceHandlers)
        {
            if (heartPieces.Count <= i) return;
            colorhandler.Initialize(heartPieces[i].pieceNumber, heartPieces[i].pieceColor);
            i++;
        }
    }

    public void SetHeartColors(ClanLogo logo) 
    {
        _heartPieceHandlers = _heartContainer.GetComponentsInChildren<HeartPieceColorHandler>();

        int i = 0;
        foreach (var piece in logo.pieceColors)
        {
            if (i >= _heartPieceHandlers.Length) break; //estetään virhe, jos värejä on enemmän kuin käsittelijöitä taulukoissa
            if (!ColorUtility.TryParseHtmlString("#" + piece, out Color colour)) colour = Color.white;
            _heartPieceHandlers[i].Initialize(i, colour);

            i++;
        }
        for (; i < _heartPieceHandlers.Length; i++) //Täydennetään logosta puuttuvt värit
        {
            _heartPieceHandlers[i].Initialize(i, Color.red); //kovakoodataan tähän hetkeksi valkoinen oletukseksi
        }
    }

    public void SetHeartColor(Color color)
    {
        _heartPieceHandlers = _heartContainer.GetComponentsInChildren<HeartPieceColorHandler>();

        int i = 0;
        foreach (HeartPieceColorHandler colorhandler in _heartPieceHandlers)
        {
            colorhandler.Initialize(i, color);
            i++;
        }
    }
}
