using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

public class CharacterSortDropdown : TMP_Dropdown
{
    public event Action<bool> DropdownStateChanged;

    protected override GameObject CreateDropdownList(GameObject template)
    {
        GameObject dropdownList = base.CreateDropdownList(template);
        DropdownStateChanged?.Invoke(true);
        return dropdownList;
    }

    protected override void DestroyDropdownList(GameObject dropdownList)
    {
        base.DestroyDropdownList(dropdownList);
        DropdownStateChanged?.Invoke(false);
    }
}

