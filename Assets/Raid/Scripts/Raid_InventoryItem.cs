using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class Raid_InventoryItem : MonoBehaviour
{
    [SerializeField] private Image ItemImage;
    [SerializeField] private TMP_Text ItemWeightText;

    public event Action<Raid_InventoryItem> OnItemClicked;

    private bool empty = true;

    public void Awake()
    {
        this.ItemImage.gameObject.SetActive(false);
        empty = true;
    }

    public void SetData(Sprite ItemSprite, int ItemWeight)
    {
        this.ItemImage.gameObject.SetActive(true);
        this.ItemImage.sprite = ItemSprite;
        this.ItemWeightText.text = ItemWeight + "kg";
        empty = false;
    }

    public void OnPointerClick(BaseEventData data)
    {
        if(empty)
        {
            return;
        }
        PointerEventData pointerData = (PointerEventData)data;
        if(pointerData.button == PointerEventData.InputButton.Left)
        {
            OnItemClicked?.Invoke(this);
        }
        else
        {
            return;
        }
    }
}
