using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgreementController : MonoBehaviour
{
    [SerializeField] private Toggle _agreementToggle;
    [SerializeField] private Button _createButton;

    // Start is called before the first frame update
    void Awake()
    {
        _createButton.interactable = false;
        _agreementToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        _createButton.interactable = isOn;
    }

    public void ResetState()
    {
        _agreementToggle.isOn = false;
        OnToggleChanged(false);
    }
}
