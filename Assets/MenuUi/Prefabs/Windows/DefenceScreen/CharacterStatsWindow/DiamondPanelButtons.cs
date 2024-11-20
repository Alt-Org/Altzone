using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DiamondPanelButtons : MonoBehaviour
{
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;

    private int value;

    private void Awake()
    {
        Debug.Log("Timanttiapaneelin awake");
        increaseButton.onClick.AddListener(OnIncreaseButtonClicked);
        decreaseButton.onClick.AddListener(OnDecreaseButtonClicked);
    } 

    private void OnIncreaseButtonClicked()
    {
        value++;
        Debug.Log("plusnappia painettu");
    }
    private void OnDecreaseButtonClicked()
    {
        value--;
        Debug.Log("miinusnappia painettu");
    }

}
