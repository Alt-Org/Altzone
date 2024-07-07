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

    private Sprite originalImage;

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


    public void ToggleImageOnAndOff(bool on)
    {
        if (on)
            GetComponent<Image>().sprite = _toggleOnImage;
        else
            GetComponent<Image>().sprite = _toggleOffImage;
    }

}
