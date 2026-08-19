using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterFilterPanel : MonoBehaviour
{
    [Header("Popup open buttons")]
    [SerializeField] private GameObject _filterPopUp;
    [SerializeField] private Button _openFilterPopUpButton1;
    [SerializeField] private Button _openFilterPopUpButton2;
    [SerializeField] private Button _openFilterPopUpButton3;

    [Header("Filter toggles")]
    [SerializeField] private Toggle _filterAcknowledged;
    [SerializeField] private Toggle _filterUnacknowledged;

    [SerializeField] private GameObject _filterAcknowledgedTextBold;
    [SerializeField] private GameObject _filterUnacknowledgedTextBold;

    [SerializeField] private Toggle _toggleAll;
    [SerializeField] private List<Toggle> _toggleList;

    [Header("Popup close buttons")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _confirmButton;


    private void Start()
    {
        _openFilterPopUpButton1.onClick.AddListener(Open);
        _openFilterPopUpButton2.onClick.AddListener(Open);
        _openFilterPopUpButton3.onClick.AddListener(Open);

        _closeButton.onClick.AddListener(Close);
        _cancelButton.onClick.AddListener(Close);

        _confirmButton.onClick.AddListener(CloseAndApply);

        _toggleAll.onValueChanged.AddListener(ToggleAllCheck);

        _filterAcknowledged.onValueChanged.AddListener(AcknowledgedToggle);
        _filterUnacknowledged.onValueChanged.AddListener(UnacknowledgedToggle);
    }

    private void ToggleAllCheck(bool toggleIsOn) //toggle all class filters on or off
    {
        if (toggleIsOn)
        {
            foreach (Toggle toggle in _toggleList)
            {
                toggle.isOn = true;
            }
        }
        else if (!toggleIsOn)
        {
            foreach (Toggle toggle in _toggleList)
            {
                toggle.isOn = false;
            }
        }
    }

    private void AcknowledgedToggle(bool toggleIsOn)
    {
        if (toggleIsOn)
        {
            _filterAcknowledgedTextBold.SetActive(true);
        }
        else if (!toggleIsOn)
        {
            _filterAcknowledgedTextBold.SetActive(false);
        }


        /*
        TextMeshProUGUI toggleText = null;
     
        if (toggleIsOn)
        {
            //toggleText.text = _filterAcknowledged.GetComponentInChildren<TextMeshProUGUI>().text;
            //_filterAcknowledged.GetComponentInChildren<TextMeshProUGUI>().text = $"<b> {toggleText} </b>";
            toggleText = _filterAcknowledgedText;

            _filterAcknowledgedText.text = $"<b> {toggleText} </b>";
        }
        else if (!toggleIsOn)
        {
            //toggleText.text = _filterAcknowledged.GetComponentInChildren<TextMeshProUGUI>().text;
            //_filterAcknowledged.GetComponentInChildren<TextMeshProUGUI>().text = toggleText.ToString();
            toggleText = _filterAcknowledgedText;

            _filterAcknowledgedText.text = toggleText.text;

        }*/

    }

    private void UnacknowledgedToggle(bool toggleIsOn)
    {
        if (toggleIsOn)
        {
            _filterUnacknowledgedTextBold.SetActive(true);
        }
        else if (!toggleIsOn)
        {
            _filterUnacknowledgedTextBold.SetActive(false);
        }
    }

    public void Open()
    {
        _filterPopUp.SetActive(true);
    }

    private void Close()
    {
        _filterPopUp.SetActive(false);
    }

    private void CloseAndApply()
    {
        //set filters
        _filterPopUp.SetActive(false);
    }
}
