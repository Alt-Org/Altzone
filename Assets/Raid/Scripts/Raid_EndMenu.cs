using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Raid_EndMenu : MonoBehaviour
{
    public static Raid_EndMenu Instance { get; private set; }

    [SerializeField]
    public RectTransform collectedFurniture;
    public RectTransform content;
    public Raid_InventoryItem itemPrefab;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite normalBackgroundSprite;
    [SerializeField] private Sprite overweightBackgroundSprite;
    [SerializeField] private GameObject normalEndResultText;
    [SerializeField] private GameObject overweightEndResultText;
    [SerializeField] private TMP_Text spaceRemainingText;
    [SerializeField] private Vector3 collectedLootItemScale = Vector3.one;

    private CanvasGroup canvasGroup;
    private bool initialized;

    public GameObject MenuRoot
    {
        get
        {
            EnsureInitialized();
            return menuRoot;
        }
    }

    private void Awake()
    {
        EnsureInitialized();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    // SetCollectedLoot for display when showing EndMenu
    public void SetCollectedLoot(List<GameFurniture> lootList)
    {
        ClearCollectedLoot();

        if (content == null || itemPrefab == null)
        {
            Debug.LogError("Cannot show collected raid loot because content or itemPrefab is missing.");
            return;
        }

        for (int i = 0; i < lootList.Count; i++)
        {
            Raid_InventoryItem UIItem = Instantiate(itemPrefab, content);
            UIItem.transform.localScale = collectedLootItemScale;
            UIItem.SetShowItemWeightText(true);
            UIItem.SetData(lootList[i]);
        }
    }

    public void SetOverWeightLimitBackground(bool wentOverWeightLimit)
    {
        if (backgroundImage != null)
        {
            Sprite selectedBackground = wentOverWeightLimit ? overweightBackgroundSprite : normalBackgroundSprite;
            if (selectedBackground != null)
            {
                backgroundImage.sprite = selectedBackground;
            }
        }
    }

    public void SetEndReasonText(ExitRaid.RaidEndReason reason)
    {
        bool wentOverWeightLimit = reason == ExitRaid.RaidEndReason.OutOfSpace;

        if (normalEndResultText != null)
        {
            normalEndResultText.SetActive(!wentOverWeightLimit);
        }

        if (overweightEndResultText != null)
        {
            overweightEndResultText.SetActive(wentOverWeightLimit);
        }
    }

    public void SetSpaceRemainingText(float currentLootWeight, float maxLootWeight)
    {
        if (spaceRemainingText == null)
        {
            return;
        }

        spaceRemainingText.text = $"{currentLootWeight:F0}kg\n/{maxLootWeight:F0}kg";
    }

    public void ReturnToLobby()
    {
        SceneManager.LoadScene("10-MenuUI");
    }
    //TODO: This is used for testing / debugging only, remove when releasing demo
    public void Restart() 
    {
        SceneManager.LoadScene("40-Raid");
    }

    private void ClearCollectedLoot()
    {
        if (content == null)
        {
            return;
        }

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    private void SetVisible(bool visible)
    {
        EnsureInitialized();

        if (!menuRoot.activeSelf)
        {
            menuRoot.SetActive(true);
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple Raid_EndMenu instances found. Using the first initialized instance.");
        }
        else
        {
            Instance = this;
        }

        if (menuRoot == null)
        {
            menuRoot = gameObject;
        }

        canvasGroup = menuRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = menuRoot.AddComponent<CanvasGroup>();
        }

        initialized = true;
    }
}
