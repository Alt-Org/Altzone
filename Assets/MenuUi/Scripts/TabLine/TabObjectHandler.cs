using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabObjectHandler : MonoBehaviour
{
    [Header("References to components")]
    [SerializeField] private Image _tabBaseComponent;

    [SerializeField] private Image _tabImageComponent;
    [SerializeField] private TextMeshProUGUI _tabText;
    [SerializeField] private TMP_FontAsset _tabTextFontDark;
    [SerializeField] private TMP_FontAsset _tabTextFontBright;

    private Color _tabColorActive = Color.white;
    private Color _tabColorInactive = Color.gray;
    private Color _textColorActive = Color.black;
    private Color _textColorInactive = Color.white;
    private TMP_FontAsset _tabTextFontActive;
    private TMP_FontAsset _tabTextFontInactive;

    private bool _active = true;

    private void Start()
    {
        if (_tabImageComponent.sprite != null) _tabText.gameObject.SetActive(false);
        else
        {
            _tabImageComponent.gameObject.SetActive(false);
            _tabText.gameObject.SetActive(true);
        }
    }

    public (Sprite, Color) SetActiveVisuals(Sprite tablineImage, Color stripeColour)
    {
        _active = true;

        if (_tabBaseComponent != null) _tabBaseComponent.color = _tabColorActive;
        if (_tabText!.gameObject.activeSelf)
        {
            if (_tabTextFontActive != null) _tabText.font = _tabTextFontActive;
            _tabText.color = _textColorActive;
        }

        return (tablineImage, stripeColour);
    }


    public void SetInactiveVisuals()
    {
        _active = false;

        if (_tabBaseComponent != null) _tabBaseComponent.color = _tabColorInactive;

        if (_tabText!.gameObject.activeSelf)
        {
            if (_tabTextFontInactive != null) _tabText.font = _tabTextFontInactive;

            _tabText.color = _textColorInactive;
        }
    }


    public void SetColour(Color activeColour, Color inactiveColour)
    {
        _tabColorActive = activeColour;
        (_textColorActive, _tabTextFontActive) = DetermineTextColour(activeColour);
        _tabColorInactive = inactiveColour;
        (_textColorInactive, _tabTextFontInactive) = DetermineTextColour(inactiveColour);

        if (_active) _tabBaseComponent.color = _tabColorActive;
        else _tabBaseComponent.color = _tabColorInactive;
    }

    private (Color, TMP_FontAsset) DetermineTextColour(Color backgroundColour)
    {
        if (backgroundColour == null) return (Color.black, _tabTextFontDark);

        Color.RGBToHSV(backgroundColour, out float h, out float s, out float v);

        if (s > 0.5f || v < 0.5f) return (Color.white, _tabTextFontBright);
        else return (Color.black, _tabTextFontDark);
    }
}
