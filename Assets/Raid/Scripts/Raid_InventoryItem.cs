using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using Photon.Pun;
using static Battle0.Scripts.Lobby.InRoom.RoomSetupManager;

public class Raid_InventoryItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image ItemImage;
    [SerializeField] public float ItemWeight;
    [SerializeField] private GameObject Lock;
    [SerializeField] private GameObject Bomb;
    [SerializeField] private GameObject BombIndicator;
    // [SerializeField] private TMP_Text ItemWeightText;

    public event Action<Raid_InventoryItem> OnItemClicked;

    private bool empty = true;

    private bool locked = false;

    private bool spectator = false;

    private bool triggered = false;

    public bool bomb = false;

    //type 0: default, type 1: lock
    public int _bombType = 0; 

    public PhotonView _photonView { get; set; }

    public void Awake()
    {
        this.ItemImage.gameObject.SetActive(false);
        empty = true;

        if ((PlayerRole)PhotonNetwork.LocalPlayer.CustomProperties["Role"] == PlayerRole.Spectator)
            spectator = true;
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
    public void SetBomb(int bombType)
    {
        _bombType = bombType;
        bomb = true;
        if (spectator)
            BombIndicator.SetActive(true);
    }
    public void TriggerBomb()
    {
        if(!triggered)
        {
            Bomb.SetActive(true);
            triggered = true;
        }
        //BombIndicator.SetActive(false);
    }
    public void SetLocked()
    {
        Lock.SetActive(true);
        locked = true;
    }

    public void OnPointerClick(PointerEventData pointerData)
    {
        if (empty || locked)
        {
            return;
        }
        if (pointerData.button == PointerEventData.InputButton.Left)
        {
            if(!spectator)
                OnItemClicked?.Invoke(this);
        }
        else
        {
            return;
        }
    }
}
