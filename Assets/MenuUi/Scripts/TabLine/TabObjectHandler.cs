using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabObjectHandler : MonoBehaviour
{
    [Header("References to components")] [SerializeField]
    private Image _tabBaseComponent;

    [SerializeField] private TextMeshProUGUI _tabTextBlack;
    [SerializeField] private TextMeshProUGUI _tabTextWhite;
    [SerializeField] private Image _darkeningImage;

    // private Color _tabColorActive = Color.gray;
    // private Color _tabColorInactive = Color.white;

    private bool _active = true;

    private void Start()
    {
        if (_darkeningImage != null)
            _darkeningImage.gameObject.SetActive(_active);

        if (_tabTextBlack != null)
            _tabTextBlack.gameObject.SetActive(!_active);

        if (_tabTextWhite != null)
            _tabTextWhite.gameObject.SetActive(_active);
    }

    public (Sprite, Color) SetActiveVisuals(Sprite tablineImage, Color stripeColour)
    {
        _active = true;

        if (_darkeningImage != null)
            _darkeningImage.gameObject.SetActive(true);

        if (_tabTextBlack != null)
            _tabTextBlack.gameObject.SetActive(false);

        if (_tabTextWhite != null)
            _tabTextWhite.gameObject.SetActive(true);

        return (tablineImage, stripeColour);
    }


    public void SetInactiveVisuals()
    {
        _active = false;

        if (_darkeningImage != null)
            _darkeningImage.gameObject.SetActive(false);

        if (_tabTextBlack != null)
            _tabTextBlack.gameObject.SetActive(true);

        if (_tabTextWhite != null)
            _tabTextWhite.gameObject.SetActive(false);
    }


    public void SetColour(Color activeColour, Color inactiveColour)
    {
        // _tabColorActive = activeColour;
        // _tabColorInactive = inactiveColour;

        if (_darkeningImage != null)
            _darkeningImage.color = activeColour;

        if (_tabBaseComponent != null)
            _tabBaseComponent.color = inactiveColour;
    }
}
