using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Raid_EndMenu : MonoBehaviour
{
    [SerializeField]
    public RectTransform collectedFurniture;
    public RectTransform content;
    public Raid_InventoryItem itemPrefab;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite normalBackgroundSprite;
    [SerializeField] private Sprite overweightBackgroundSprite;
    [SerializeField] private GameObject normalEndResultText;
    [SerializeField] private GameObject overweightEndResultText;
    [SerializeField] private TMP_Text spaceRemainingText;
    [SerializeField] private Vector3 collectedLootItemScale = Vector3.one;

    // SetCollectedLoot for display when showing EndMenu
    public void SetCollectedLoot(List<GameFurniture> lootList)
    {
        ClearCollectedLoot();

        for (int i = 0; i < lootList.Count; i++)
        {
            Transform itemParent = CreateCollectedLootItemContainer();
            Raid_InventoryItem UIItem = Instantiate(itemPrefab, itemParent);
            UIItem.transform.localScale = collectedLootItemScale;
            RectTransform itemTransform = UIItem.GetComponent<RectTransform>();
            itemTransform.anchorMin = new Vector2(0.5f, 0.5f);
            itemTransform.anchorMax = new Vector2(0.5f, 0.5f);
            itemTransform.anchoredPosition = Vector2.zero;
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

    private Transform CreateCollectedLootItemContainer()
    {
        GameObject itemContainer = new GameObject("CollectedLootItem", typeof(RectTransform));
        RectTransform itemContainerTransform = itemContainer.GetComponent<RectTransform>();
        itemContainerTransform.SetParent(content, false);
        return itemContainerTransform;
    }

}
