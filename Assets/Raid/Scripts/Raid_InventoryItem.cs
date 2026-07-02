using Altzone.Scripts;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using Altzone.Scripts.Model.Poco.Game;

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
    private RectTransform itemBallRect;
    private Raid_PointerClickBlocker itemBallClickBlocker;
    private HorizontalLayoutGroup layoutGroup;
    private Raid_Timer raidTimer;
    private Vector2 endLoc;
    private float t = 0f;
    [SerializeField] private float speed = 15f;
    [SerializeField] private TMP_Text ItemWeightPopUp;
    [SerializeField] private TMP_Text ItemWeightText;
    [SerializeField] private bool showItemWeightText = false;

    public event Action<Raid_InventoryItem> OnItemClicked;

    private bool moving = false;

    private bool empty = true;

    private bool locked = false;

    private bool active = true;

    private bool triggered = false;

    public bool bomb = false;
    private bool showTrapIndicator = true;

    private bool timeEnded = false;
    public GameFurniture furnitureData;

    private AudioSource audioSource;
#pragma warning disable CS0612
    [SerializeField] private SetVolume volumeSettings;
#pragma warning restore CS0612
    private SettingsCarrier.SoundType soundType;
    private bool hasSoundType;
    public AudioClip pickUp;
    public AudioClip explosion;

    [Tooltip("Trap type: 0 = end raid, 1 = freeze, 2 = double next loot weight.")]
    public int _bombType = 0; 
    public Sprite CurrentItemSprite => ItemImage != null ? ItemImage.sprite : null;
    private Sprite pendingRecentLootSprite;
    private Action<Sprite> onLootBallArrived;

    public void Awake()
    {
        ResolveLocalReferences();

        if (empty && ItemImage != null)
        {
            ItemImage.gameObject.SetActive(false);
        }

        RefreshBombIndicator();
    }

    public void ConfigureSharedReferences(Raid_References references, GameObject heart, Raid_Timer timer)
    {
        if (raid_References == null)
        {
            raid_References = references;
        }

        if (Heart == null)
        {
            Heart = heart;
        }

        target = Heart != null && Heart.TryGetComponent(out RectTransform heartRect)
            ? heartRect
            : null;

        SetRaidTimer(timer);
    }

    private void OnDestroy()
    {
        if (raidTimer != null)
        {
            raidTimer.TimeEnded -= OnTimeEnded;
        }
    }

    public void Update()
    {
        if (moving)
        {
            BallToHeart();
        }
    }

    public void SetData(GameFurniture gameFurniture)
    {
        if (gameFurniture == null || ItemImage == null)
        {
            return;
        }

        ItemWeight = (float)gameFurniture.Weight;
        furnitureData = gameFurniture;
        ItemImage.gameObject.SetActive(true);
        ItemImage.sprite = gameFurniture.FurnitureInfo?.Image;
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
            if(_bombType == 1 && Bomb != null)
            {
                Bomb.transform.localScale = new Vector2(3f, 3f);
            }
            SetTriggeredTrapSprite();
            if (Bomb != null)
            {
                Bomb.SetActive(true);
            }
            triggered = true;
            PlayClip(explosion);
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
        if (ItemBall == null || itemBallRect == null || Heart == null || target == null)
        {
            lootBallArrived?.Invoke(recentLootSprite);
            return;
        }

        pendingRecentLootSprite = recentLootSprite;
        onLootBallArrived = lootBallArrived;
        if (ItemWeightText != null)
        {
            ItemWeightText.gameObject.SetActive(false);
        }

        PlayClip(pickUp);
        Vector3 offset = new Vector3(target.rect.width / 2, -target.rect.height / 2, 0f);
        endLoc = target.position + offset;

        ItemBall.transform.SetParent(Heart.transform);
        SetItemBallClickBlocker(true);
        moving = true;
        t = 0f;

        SetItemWeightPopUpVisible(true);
    }
    public void BallToHeart()
    {
        if (itemBallRect == null)
        {
            SetItemBallClickBlocker(false);
            moving = false;
            return;
        }

        t += speed * Time.deltaTime;
        float step = Mathf.SmoothStep(0f, 1f, t * Time.deltaTime);
        Vector2 newPosition = Vector2.Lerp(itemBallRect.anchoredPosition, endLoc, step);
        itemBallRect.anchoredPosition = newPosition;

        if (Vector2.Distance(itemBallRect.anchoredPosition, endLoc) <= 20f)
        {
            moving = false;
            onLootBallArrived?.Invoke(pendingRecentLootSprite);
            pendingRecentLootSprite = null;
            onLootBallArrived = null;
            SetItemBallClickBlocker(false);
            if (ItemBall != null)
            {
                ItemBall.SetActive(false);
            }
        }
    }
    public void SetLocked()
    {
        if (Lock != null)
        {
            Lock.SetActive(true);
        }
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
            if (active)
            {
                OnItemClicked?.Invoke(this);
            }       
        }
        else
        {
            return;
        }
    }
    private void OnTimeEnded()
    {
        timeEnded = true;
        active = false;
    }

    private void SetBGColor()
    {
        int spriteIndex;
        switch (ItemWeight)
        {
            case <= 1.0f:
                spriteIndex = 1;
                break;
            case <= 7.5f:
                spriteIndex = 0;
                break;
            case <= 40.0f:
                spriteIndex = 4;
                break;
            case <= 70.0f:
                spriteIndex = 3;
                break;
            default:
                spriteIndex = 2;
                break;
        }

        SetSpriteIfAvailable(Bubble, Bubbles, spriteIndex);
        SetSpriteIfAvailable(Aura, Auras, spriteIndex);
    }

    private void ResolveLocalReferences()
    {
        TryGetComponent(out audioSource);
        ResolveSoundType();
        TryGetComponent(out layoutGroup);
        itemBallRect = ItemBall != null && ItemBall.TryGetComponent(out RectTransform rect)
            ? rect
            : null;
        itemBallClickBlocker = ItemBall != null && ItemBall.TryGetComponent(out Raid_PointerClickBlocker clickBlocker)
            ? clickBlocker
            : null;

        SetItemBallClickBlocker(false);
    }

    private void SetRaidTimer(Raid_Timer timer)
    {
        if (raidTimer == timer)
        {
            return;
        }

        if (raidTimer != null)
        {
            raidTimer.TimeEnded -= OnTimeEnded;
        }

        raidTimer = timer;

        if (raidTimer != null)
        {
            raidTimer.TimeEnded += OnTimeEnded;
        }
    }

    private void SetItemBallClickBlocker(bool blocksClicks)
    {
        if (ItemBall == null)
        {
            return;
        }

        if (itemBallClickBlocker == null && !ItemBall.TryGetComponent(out itemBallClickBlocker))
        {
            itemBallClickBlocker = ItemBall.AddComponent<Raid_PointerClickBlocker>();
        }

        itemBallClickBlocker.enabled = blocksClicks;
    }

    private static void SetSpriteIfAvailable(Image image, Sprite[] sprites, int index)
    {
        if (image == null || sprites == null || index < 0 || index >= sprites.Length || sprites[index] == null)
        {
            return;
        }

        image.sprite = sprites[index];
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

    private void SetItemWeightPopUpVisible(bool visible)
    {
        if (ItemWeightPopUp == null)
        {
            return;
        }

        ItemWeightPopUp.gameObject.SetActive(visible);
        if (visible)
        {
            ItemWeightPopUp.text = $"+{ItemWeight}kg";
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null || SettingsCarrier.Instance == null || !hasSoundType)
        {
            return;
        }

        audioSource.PlayOneShot(clip, SettingsCarrier.Instance.SentVolume(soundType));
    }

    private void ResolveSoundType()
    {
        if (hasSoundType)
        {
            return;
        }

        if (volumeSettings == null)
        {
            TryGetComponent(out volumeSettings);
        }

        if (volumeSettings != null)
        {
            soundType = volumeSettings._soundType;
            hasSoundType = true;
        }
    }
}
