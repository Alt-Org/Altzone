using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropDownMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _dropDownPanel;
    [SerializeField]
    private bool _dropDownOpen = false;



    private void Start()
    {
        _dropDownPanel.SetActive(false);
        _dropDownOpen = false;

    }

    private void OnEnable()
    {
        _dropDownPanel.SetActive(false);
        _dropDownOpen = false;
    }

    public void PressedHamburgerButton()
    {

        if (!_dropDownOpen)
        {
            _dropDownPanel.SetActive(true);
            _dropDownOpen = true;

        }
        else if (_dropDownOpen)
        {

            _dropDownPanel.SetActive(false);
            _dropDownOpen = false;
        }

    }


    public void DeselectMenu()
    {
        StartCoroutine(WaitMenuClose()); //wait so that the buttons have time to register the input
    }

    IEnumerator WaitMenuClose()
    {

        //Wait for 4 seconds
        yield return new WaitForSecondsRealtime(0.3f);

        _dropDownPanel.SetActive(false);
        _dropDownOpen = false;

    }

}
