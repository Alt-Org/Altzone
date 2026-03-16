using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinFigureScript : MonoBehaviour, ISoulHomeObjectClick
{
    [SerializeField] GameObject figure;
    private GameObject box;
    private bool boxOpened = false;
    [SerializeField] KaikkiOnHyvinBoxScript kaikkiOnHyvinBoxScript;

    public void HandleClick()
    {
        if (boxOpened)
        {return;}
        box = figure.transform.GetChild(0).gameObject;
        kaikkiOnHyvinBoxScript.boxHandler(box);
        boxOpened = true;
    }
}
}