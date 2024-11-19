using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EsineDisplay : MonoBehaviour
{
    public KauppaItems items;
    public TextMeshProUGUI hinta;

    public Image esineenKuva;
    void Start()
    {
        hinta.text = items.hinta;

        esineenKuva.sprite = items.esine;
    }

    public void PassItemToVoting()
    {
        VotingActions.PassKauppaItem?.Invoke(this);
    }
}
