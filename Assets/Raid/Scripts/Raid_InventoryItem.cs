using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using Photon.Pun;

public class Raid_InventoryItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image ItemImage;
    [SerializeField] public float ItemWeight;
    // [SerializeField] private TMP_Text ItemWeightText;

    public event Action<Raid_InventoryItem> OnItemClicked;

    private bool empty = true;

    public PhotonView _photonView { get; set; }

    public void Awake()
    {
        this.ItemImage.gameObject.SetActive(false);
        empty = true;
    }

    public void SetData(Sprite ItemSprite, float LootItemWeight)
    {
        this.ItemImage.gameObject.SetActive(true);
        this.ItemImage.sprite = ItemSprite;
        // this.ItemWeightText.text = ItemWeight + "kg";
        empty = false;
    }

    public void RemoveData()
    {
        this.ItemImage.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData pointerData)
    {
        if (empty)
        {
            return;
        }
        if (pointerData.button == PointerEventData.InputButton.Left)
        {
            OnItemClicked?.Invoke(this);
        }
        else
        {
            return;
        }
    }
}
