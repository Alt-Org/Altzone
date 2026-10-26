using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MenuUI.Scripts.SoulHome;
using Altzone.Scripts.ReferenceSheets;

public class SetSelectMenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject _setSelectMenu;
    [SerializeField] private ScrollRect _setSelectMenuScrollRect;
    [SerializeField] private FurnitureTrayHandler _furnitureTrayHandler;
    [SerializeField] private FurnitureTrayCategoryButton _decorateModeSetsButton;
    [SerializeField] private GameObject _setSelectButtonContainer;
    [SerializeField] private GameObject _setSelectButtonPrefab;
    [SerializeField] private StorageFurnitureReference _storageFurnitureReference;
    void Awake()
    {
        foreach (FurnitureSetInfo setInfo in _storageFurnitureReference.Info) // creates a set filter button for each set
        {
            GameObject _setSelectButton = Instantiate(_setSelectButtonPrefab, _setSelectButtonContainer.transform);
            if (setInfo.SetFontName != null) _setSelectButton.GetComponent<SetSelectButtonHandler>().SetFontSprite(setInfo.SetFontName);
            else _setSelectButton.GetComponent<SetSelectButtonHandler>().SetSetName(setInfo.SetName);
            _setSelectButton.GetComponent<SetSelectButtonHandler>().Button.onClick.AddListener(() => SelectSet(setInfo.SetName));
        }
    }
    public void ToggleMenuVisibility()
    {
        _setSelectMenu.SetActive(!_setSelectMenu.activeSelf);
    }
    public void SelectSet(string set)
    {
        if (set == "") // Clears the set filter if empty
        {
            _furnitureTrayHandler._currentSelectedSet = null; 
            _decorateModeSetsButton.DeSelect();
        }
        else
        {
            _furnitureTrayHandler._currentSelectedSet = set;
            _decorateModeSetsButton.Select(); // if set filter active, make the button "Selected"
        }
        _furnitureTrayHandler.FilterTrayObjects(_furnitureTrayHandler._currentSelectedCategory);
        _setSelectMenu.SetActive(false);
        _setSelectMenu.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f; // reset scroll to the top
    }
}
