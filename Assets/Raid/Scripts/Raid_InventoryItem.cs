using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using Altzone.Scripts.Model.Poco.Game;
//using Photon.Pun;
//using static MenuUI.Scripts.Lobby.InRoom.RoomSetupManager;

public class Raid_InventoryItem : MonoBehaviour, IPointerClickHandler
{
    private static readonly Vector2 LossHaloPadding = new Vector2(70f, 70f);
    private static readonly Vector2 LossHaloOffset = Vector2.zero;
    private const int CollectedLootLayoutPaddingLeft = 25;
    private const int CollectedLootLayoutPaddingRight = 25;
    private const int CollectedLootLayoutPaddingTop = 15;
    private const int CollectedLootLayoutPaddingBottom = 125;
    private static readonly Vector2 CollectedLootWeightTextPosition = new Vector2(0f, -500f);
    private static readonly Vector2 CollectedLootWeightTextSize = new Vector2(350f, 185f);

    [SerializeField] private Image ItemImage;
    [SerializeField] public float ItemWeight;
    [SerializeField] private GameObject Lock;
    [SerializeField] private GameObject Bomb;
    [SerializeField] private Image BombImage;
    [SerializeField] private Animator BombAnimator;
    [SerializeField] private Sprite[] TrapSprites;
    [SerializeField] private GameObject BombIndicator;
    [SerializeField] private GameObject ItemBall;
    [SerializeField] private GameObject Heart;
    [SerializeField] public Image Aura;
    [SerializeField] public Image Bubble;
    [SerializeField] public Sprite[] Auras;
    [SerializeField] public Sprite[] Bubbles;
    [SerializeField] public Raid_References raid_References;
    
    private RectTransform target;
    Vector2 endLoc;
    Vector3 offset;
    private float startTime;
    private float journeyLength;
    private float t = 0f;
    [SerializeField] private float speed = 15f;
    [SerializeField] private TMP_Text ItemWeightPopUp;
    [SerializeField] private TMP_Text ItemWeightText;
    [SerializeField] private bool showItemWeightText = false;

    public event Action<Raid_InventoryItem> OnItemClicked;

    private bool moving = false;

    private bool empty = true;

    private bool locked = false;

    private bool spectator = false;

    private bool active = true;

    private bool triggered = false;

    public bool bomb = false;
    private bool showTrapIndicator = true;

    private bool timeEnded = false;
    public GameFurniture furnitureData;

    private AudioSource audioSource;
    public AudioClip pickUp;
    public AudioClip explosion;

    //type 0: default, type 1: lock
    public int _bombType = 0; 
    public Sprite CurrentItemSprite => ItemImage != null ? ItemImage.sprite : null;
    private Sprite pendingRecentLootSprite;
    private Action<Sprite> onLootBallArrived;

    //public PhotonView _photonView { get; set; }
    public void Awake()
    {
        Heart = GameObject.FindWithTag("Heart");
        target = Heart.GetComponent<RectTransform>();

        raid_References = GameObject.Find("ScriptHolder").GetComponent<Raid_References>();

        Raid_Timer raidTimer = FindObjectOfType<Raid_Timer>();
        if (raidTimer != null)
        {
            raidTimer.TimeEnded += OnTimeEnded;
        }
        audioSource = GetComponent<AudioSource>();
        if (empty)
        {
            ItemImage.gameObject.SetActive(false);
        }
        RefreshBombIndicator();

        //if ((PlayerRole)PhotonNetwork.LocalPlayer.CustomProperties["Role"] == PlayerRole.Spectator)
            spectator = false;
    }
    public void Update()
    {
        if (moving)
            BallToHeart();
    }

    // Sets this slots data based on contents of GameFurniture
    public void SetData(GameFurniture gameFurniture)
    {
        ItemWeight = (float)gameFurniture.Weight;
        furnitureData = gameFurniture;
        ItemImage.gameObject.SetActive(true);
        ItemImage.sprite = gameFurniture.FurnitureInfo.Image;
        SetItemWeightTextActive(showItemWeightText);
        empty = false;
        SetBGColor();
    }

    public void SetShowItemWeightText(bool show)
    {
        showItemWeightText = show;
        SetItemWeightTextActive(showItemWeightText);
    }

    public void SetCollectedLootDisplayLayout()
    {
        HorizontalLayoutGroup layoutGroup = GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.padding = new RectOffset(
                CollectedLootLayoutPaddingLeft,
                CollectedLootLayoutPaddingRight,
                CollectedLootLayoutPaddingTop,
                CollectedLootLayoutPaddingBottom);
        }

        if (ItemWeightText == null)
        {
            return;
        }

        RectTransform weightTextRect = ItemWeightText.rectTransform;
        weightTextRect.anchorMin = new Vector2(0.5f, 1f);
        weightTextRect.anchorMax = new Vector2(0.5f, 1f);
        weightTextRect.pivot = new Vector2(0.5f, 0.5f);
        weightTextRect.anchoredPosition = CollectedLootWeightTextPosition;
        weightTextRect.sizeDelta = CollectedLootWeightTextSize;
    }

    public void SetLossHaloVisible(bool visible)
    {
        if (ItemImage == null)
        {
            return;
        }

        Raid_RedHalo.SetVisible(ItemImage.gameObject, visible, LossHaloPadding, LossHaloOffset);
    }

    public void RemoveData()
    {
        SetLossHaloVisible(false);

        if (ItemImage != null)
        {
            ItemImage.sprite = null;
            ItemImage.gameObject.SetActive(false);
        }

        ItemWeight = 0f;
        furnitureData = null;
        empty = true;
        bomb = false;
        RefreshBombIndicator();
    }
    public void SetBomb(int bombType)
    {
        SetTrap(bombType);
    }

    public void SetTrap(int trapType)
    {
        _bombType = trapType;
        bomb = true;
        triggered = false;
        RefreshBombIndicator();
    }
    public void TriggerBomb()
    {
        TriggerTrap();
    }

    public void TriggerTrap()
    {
        if(!triggered && !timeEnded)
        {
            string trapName;
            switch (_bombType)
            {
                case 0:
                    trapName = "EndGame";
                    break;
                case 1:
                    trapName = "Freeze";
                    break;
                case 2:
                    trapName = "DoubleWeight";
                    break;
                default:
                    trapName = "Unknown";
                    break;
            }
            Debug.Log($"(RAID) Trap triggered: {trapName} (type {_bombType})");
            if(_bombType == 1)
            {
                Bomb.transform.localScale = new Vector2(3f, 3f);
            }
            SetTriggeredTrapSprite();
            Bomb.SetActive(true);
            triggered = true;
            audioSource.PlayOneShot(explosion, SettingsCarrier.Instance.SentVolume(GetComponent<SetVolume>()._soundType));
        }
        RefreshBombIndicator();
    }

    public void SetTrapIndicatorVisible(bool visible)
    {
        showTrapIndicator = visible;
        RefreshBombIndicator();
    }

    public void SetAuraVisible(bool visible)
    {
        if (Aura == null)
        {
            return;
        }

        Aura.gameObject.SetActive(visible);
    }

    public void SetBubbleVisible(bool visible)
    {
        if (Bubble == null)
        {
            return;
        }

        Bubble.gameObject.SetActive(visible);
    }

    private void RefreshBombIndicator()
    {
        if (BombIndicator == null)
        {
            return;
        }

        BombIndicator.SetActive(bomb && !triggered && showTrapIndicator);
    }

    private void SetTriggeredTrapSprite()
    {
        if (BombAnimator == null && Bomb != null)
        {
            BombAnimator = Bomb.GetComponent<Animator>();
        }

        if (BombAnimator != null)
        {
            BombAnimator.enabled = false;
        }

        if (BombImage == null && Bomb != null)
        {
            BombImage = Bomb.GetComponent<Image>();
        }

        if (BombImage == null || TrapSprites == null || _bombType < 0 || _bombType >= TrapSprites.Length)
        {
            return;
        }

        Sprite trapSprite = TrapSprites[_bombType];
        if (trapSprite == null)
        {
            return;
        }

        BombImage.sprite = trapSprite;
        BombImage.preserveAspect = true;
    }
    public void LaunchBall(Sprite recentLootSprite = null, Action<Sprite> lootBallArrived = null)
    {
        pendingRecentLootSprite = recentLootSprite;
        onLootBallArrived = lootBallArrived;
        ItemWeightText.gameObject.SetActive(false);
        audioSource.PlayOneShot(pickUp, SettingsCarrier.Instance.SentVolume(GetComponent<SetVolume>()._soundType));
        offset = new Vector3(target.rect.width / 2, -target.rect.height / 2, 0f);
        endLoc = target.position + offset;

        ItemBall.transform.SetParent(Heart.transform);
        moving = true;

        ItemWeightPopUp.gameObject.SetActive(true);
        ItemWeightPopUp.text = ("+" + ItemWeight + "kg");
    }
    public void BallToHeart()
    {
        t += speed * Time.deltaTime;
        float step = Mathf.SmoothStep(0f, 1f, t * Time.deltaTime);
        Vector2 newPosition = Vector2.Lerp(ItemBall.GetComponent<RectTransform>().anchoredPosition, endLoc, step);
        ItemBall.GetComponent<RectTransform>().anchoredPosition = newPosition;

        if (Vector2.Distance(ItemBall.GetComponent<RectTransform>().anchoredPosition, endLoc) <= 20f)
        {
            moving = false;
            onLootBallArrived?.Invoke(pendingRecentLootSprite);
            pendingRecentLootSprite = null;
            onLootBallArrived = null;
            ItemBall.SetActive(false);
        }
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
            if (!spectator && active)
            {
                OnItemClicked?.Invoke(this);
            }       
        }
        else
        {
            return;
        }
    }
    void OnTimeEnded()
    {
        timeEnded = true;
        active = false;
    }
    void SetBGColor()
    {
        Debug.Log("SetBGColorissa: " + ItemWeight);
        switch (ItemWeight)
        {
            case <= 1.0f:
                Bubble.sprite = Bubbles[1];
                Aura.sprite = Auras[1];
                break;
            case <= 7.5f:
                Bubble.sprite = Bubbles[0];
                Aura.sprite = Auras[0];
                break;
            case <= 40.0f:
                Bubble.sprite = Bubbles[4];
                Aura.sprite = Auras[4];
                break;
            case <= 70.0f:
                Bubble.sprite = Bubbles[3];
                Aura.sprite = Auras[3];
                break;
            default:
                Bubble.sprite = Bubbles[2];
                Aura.sprite = Auras[2];
                break;

        }

            

    }

    private void SetItemWeightTextActive(bool isActive)
    {
        if (ItemWeightText == null)
        {
            return;
        }

        ItemWeightText.gameObject.SetActive(isActive);
        ItemWeightText.text = $"{ItemWeight:F0} kg";
    }
}
