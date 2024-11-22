using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DiamondPanelButtons : MonoBehaviour
{

    //Tee tähän serielisefield jokaiselle napille
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;

    private int value;

    private void Awake()
    {
        Debug.Log("Timanttiapaneelin awake");
        increaseButton.onClick.AddListener(OnPlusButtonClicked);
        decreaseButton.onClick.AddListener(OnMinusButtonClicked);
    } 


    //Kutsutaan vaan metodia olion nimellä toisessa scriptissä
    private void OnPlusButtonClicked()
    {
        value++;
        Debug.Log("plusnappia painettu");
    }
    private void OnMinusButtonClicked()
    {
        value--;
        Debug.Log("miinusnappia painettu");
    }

}
