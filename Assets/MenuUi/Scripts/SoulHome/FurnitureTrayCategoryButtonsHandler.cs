using System;
using UnityEngine;

public class FurnitureTrayCategoryButtonsHandler : MonoBehaviour
{
    [SerializeField] private FurnitureTrayCategoryButton _defaultButton;
    [SerializeField] private FurnitureTrayCategoryButton _floorButton;
    [SerializeField] private FurnitureTrayCategoryButton _wallButton;
    [SerializeField] private FurnitureTrayCategoryButton _ceilingButton;
    [SerializeField] private FurnitureTrayCategoryButton _specialButton;

    public event Action<FurnitureTrayCategoryButton> OnCategoryButtonSelected;

    public bool SelectCategory(int category)
    {
        FurnitureTrayCategoryButton button;
        switch (category)
        {
            case 0: button = _floorButton; break;
            case 1: button = _defaultButton; break; // "All", does not exist in renovate mode
            case 2: button = _specialButton; break; // Frames in renovate mode
            case 3: button = _ceilingButton; break; // Does not exist in renovate mode
            case 4: button = _wallButton; break;
            default: button = null; break;
        }

        if (button == null) return false;
        OnCategoryButtonSelected?.Invoke(button);
        return true;
    }
}