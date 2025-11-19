using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EsineDisplay : MonoBehaviour
{
    public KauppaItems items;
    public TextMeshProUGUI price;
    public Image contentImage;

    void Start()
    {
        price.text = items.hinta;

        contentImage.sprite = items.esine;
    }

    public void PassItemToVoting()
    {
        //VotingActions.PassKauppaItem?.Invoke(this);
    }

}
