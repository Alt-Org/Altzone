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

    private Color _tabColorActive = Color.white;
    private Color _tabColorInactive = Color.gray;

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

    public Sprite SetActiveVisuals(Sprite tablineImage)
    {
        _active = true;
        if (_tabBaseComponent != null) _tabBaseComponent.color = _tabColorActive;
        return tablineImage;
    }


    public void SetInactiveVisuals()
    {
        _active = false;
        if (_tabBaseComponent != null) _tabBaseComponent.color = _tabColorInactive;
    }


    public void SetColour(Color activeColour, Color inactiveColour)
    {
        _tabColorActive = activeColour;
        _tabColorInactive = inactiveColour;
        if (_active) _tabBaseComponent.color = _tabColorActive;
        else _tabBaseComponent.color = _tabColorInactive;
    }
}
