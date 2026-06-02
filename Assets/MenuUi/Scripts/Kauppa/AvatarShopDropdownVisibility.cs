using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AvatarShopDropdownVisibility : MonoBehaviour
{
    [SerializeField] GameObject dropdown;
    private void OnEnable()
    {
        dropdown.SetActive(true);
    }

    private void OnDisable()
    {
        dropdown.SetActive(false);
    }

}
