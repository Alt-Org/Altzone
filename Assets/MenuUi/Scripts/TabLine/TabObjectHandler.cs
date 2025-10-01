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
    [SerializeField] private Image _tabDarkeningImage;
    [SerializeField] private TextMeshProUGUI _tabText;

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
        if (_tabDarkeningImage != null) _tabDarkeningImage.gameObject.SetActive(false);
        return tablineImage;
    }


    public void SetInactiveVisuals()
    {
        if (_tabDarkeningImage != null) _tabDarkeningImage.gameObject.SetActive(true);
    }


    public void SetColour(Color colour)
    {
        if (_tabImageComponent != null) _tabBaseComponent.color = colour;
    }
}
