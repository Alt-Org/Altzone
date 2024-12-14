using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;

public class ClanHeartColorSetter : MonoBehaviour
{
    [SerializeField] private Transform _heartContainer;
    private HeartPieceColorHandler[] _heartPieceHandlers = { };

    private void OnEnable()
    {
        if (ServerManager.Instance.Clan == null) return;

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

    public void SetHeartColors(List<HeartPieceData> heartPieces)
    {
        _heartPieceHandlers = _heartContainer.GetComponentsInChildren<HeartPieceColorHandler>();

        int i = 0;
        foreach (HeartPieceColorHandler colorhandler in _heartPieceHandlers)
        {
            colorhandler.Initialize(heartPieces[i].pieceNumber, heartPieces[i].pieceColor);
            i++;
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
