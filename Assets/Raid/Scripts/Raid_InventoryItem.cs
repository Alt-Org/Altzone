using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class Raid_InventoryItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image ItemImage;
    [SerializeField] public float ItemWeight;
    [SerializeField] private GameObject Lock;
    [SerializeField] private GameObject Bomb;
    [SerializeField] private GameObject BombIndicator;
    [SerializeField] private GameObject ItemBall;
    [SerializeField] private GameObject Heart;
    [SerializeField] public Image Aura;
    [SerializeField] public Image Bubble;
    [SerializeField] public Sprite[] Auras;
    [SerializeField] public Sprite[] Bubbles;

    private RectTransform target;
    Vector2 endLoc;
    Vector3 offset;
    private float startTime;
    private float journeyLength;
    private float t = 0f;
    [SerializeField] private float speed = 15f;
    [SerializeField] private TMP_Text ItemWeightPopUp;
    [SerializeField] private TMP_Text ItemWeightText;

    public event Action<Raid_InventoryItem> OnItemClicked;

    private bool moving = false;

    private bool empty = true;

    private bool locked = false;

    private bool spectator = false;

    private bool active = true;

    private bool triggered = false;

    public bool bomb = false;

    private bool timeEnded = false;

    private AudioSource audioSource;
    public AudioClip pickUp;
    public AudioClip explosion;

    // type 0: default, type 1: lock
    public int _bombType = 0;

    public void Awake()
    {
        Heart = GameObject.FindWithTag("Heart");
        target = Heart.GetComponent<RectTransform>();

        Raid_Timer raidTimer = FindObjectOfType<Raid_Timer>();
        if (raidTimer != null)
        {
            raidTimer.TimeEnded += OnTimeEnded;
        }
        audioSource = GetComponent<AudioSource>();
        ItemImage.gameObject.SetActive(false);
        empty = true;

            spectator = true;
    }
    public void Update()
    {
        if (moving)
            BallToHeart();
    }

    public void SetData(Sprite ItemSprite, float LootItemWeight)
    {
        ItemImage.gameObject.SetActive(true);
        ItemImage.sprite = ItemSprite;
        ItemWeight = LootItemWeight;
        ItemWeightText.text = ItemWeight + "kg";
        empty = false;
        SetBGColor();
    }

    public void RemoveData()
    {
        ItemImage.gameObject.SetActive(false);
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
        if(!triggered && !timeEnded)
        {
            if(_bombType == 1)
            {
                Bomb.transform.localScale = new Vector2(3f, 3f);
            }
            Bomb.SetActive(true);
            triggered = true;
            audioSource.PlayOneShot(explosion, SettingsCarrier.Instance.SentVolume(GetComponent<SetVolume>()._soundType));
        }
    }
    public void LaunchBall()
    {
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

        var rt = ItemBall?.GetComponent<RectTransform>();
        if (rt == null) return;

        Vector2 newPosition = Vector2.Lerp(rt.anchoredPosition, endLoc, step);
        rt.anchoredPosition = newPosition;

        if (Vector2.Distance(rt.anchoredPosition, endLoc) <= 20f)
        {
            moving = false;
            var heartScript = Heart?.GetComponent<HeartScript>();
            if (heartScript != null) heartScript.UpdateColor();

            if (ItemBall != null) ItemBall.SetActive(false);
        }
    }
    public void SetLocked()
    {
        Lock.SetActive(true);
        locked = true;
    }

    public void SetSpectator(bool isSpectator)
    {
        spectator = isSpectator;
        if (spectator && BombIndicator != null && bomb)
            BombIndicator.SetActive(true);
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
}
