using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add a special onClick function for each child of an object (Shop)
/// </summary>
public class ShopItemsClickController : MonoBehaviour
{
    [SerializeField] private GameObject _confirmationPopUp;

    private void Awake()
    {
        var _shopItems = GetComponentsInChildren<Button>();
        if (_shopItems.Length == 0)
            return;

        foreach (var item in _shopItems)
        {
            item.onClick.AddListener(OpenPopUp);
            item.onClick.AddListener(delegate () { item.GetComponent<EsineDisplay>().PassItemToVoting(); });
        }
    }
    private void OpenPopUp() => _confirmationPopUp.SetActive(true);
}
