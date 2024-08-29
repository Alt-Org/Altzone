using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EsineDisplay : MonoBehaviour
{
    public KauppaItems items;
    public TextMeshProUGUI hinta;

    private SettingsCarrier settingsCarrier = SettingsCarrier.Instance;

    public Image esineenKuva;
    void Start()
    {
        hinta.text = items.hinta;

        esineenKuva.sprite = items.esine;
    }



    public void PassItemToVoting()
    {
        settingsCarrier.ItemSetForVoting(this);

        //Debug.Log("Item is set for voting: " + settingsCarrier.GetCurrentVoteItem().ToString());
    }
}
