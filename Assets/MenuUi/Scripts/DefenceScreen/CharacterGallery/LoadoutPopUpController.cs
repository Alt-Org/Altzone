using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadoutPopUpController : MonoBehaviour
{


    [SerializeField] private GameObject _loadoutPopUp;
    [SerializeField] private Button _openLoadoutPopUpButton1;
    [SerializeField] private Button _openLoadoutPopUpButton2;
    [SerializeField] private Button _openLoadoutPopUpButton3;


    private void Start()
    {
        _openLoadoutPopUpButton1.onClick.AddListener(TabOpen);
        _openLoadoutPopUpButton2.onClick.AddListener(TabOpen);
        _openLoadoutPopUpButton3.onClick.AddListener(TabOpen);
    }

    public void Open()
    {
        _loadoutPopUp.SetActive(true);    
    }

    public void Close()
    {
        _loadoutPopUp.SetActive(false);        
    }

    private void TabOpen()
    {
        if (!_loadoutPopUp.activeSelf)
        {
            Open();
        }
        else if (_loadoutPopUp.activeSelf)
        {
            Close();
        }
    }
}

