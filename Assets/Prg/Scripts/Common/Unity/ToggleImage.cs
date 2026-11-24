using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleImage : MonoBehaviour
{

    [SerializeField]
    private Sprite _toggleOnImage;

    [SerializeField]
    private Sprite _toggleOffImage;

    [SerializeField] private Toggle _toggle;
    [SerializeField] private Image _iconToToggle;

    private Sprite originalImage;

    private void Awake()
    {
        if (_toggle == null)
            _toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        if (_toggle == null) return;

        _toggle.onValueChanged.AddListener(OnToggleChanged);
        OnToggleChanged(_toggle.isOn);
    }

    private void OnDisable()
    {
        if (_toggle != null)
            _toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    public void RefreshImage()
    {
        if (_toggle == null) return;

        ToggleImageOnAndOff(_toggle.isOn);
    }

    private void SetOriginalImage()
    {
        if (GetComponentInParent<Toggle>().isOn == true)
            originalImage = _toggleOnImage;
        else
            originalImage = _toggleOffImage;
    }


    private void Start()
    {
        SetOriginalImage();
        GetComponent<Image>().sprite = originalImage;
    }


    public void ToggleImageOnAndOff(bool isOn)
    {
        /*if (on)
            GetComponent<Image>().sprite = _toggleOnImage;
        else
            GetComponent<Image>().sprite = _toggleOffImage;*/

        if (_iconToToggle == null) return;

        _iconToToggle.enabled = true;
        _iconToToggle.sprite = isOn ? _toggleOnImage : _toggleOffImage;
    }

    private void OnToggleChanged(bool isOn)
    {
        ToggleImageOnAndOff(isOn);
    }

}
