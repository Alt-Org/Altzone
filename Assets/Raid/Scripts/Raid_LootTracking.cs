using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using System;

public class Raid_LootTracking : MonoBehaviour
{
    [SerializeField, Header("Reference scripts")] private Raid_References raid_References;
    [SerializeField] private ExitRaid exitRaid;

    [SerializeField, Header("Reference game components")] private TMP_Text HeartLootText;
    [SerializeField] private Sprite BrokenHeartSprite;
    
    [SerializeField, Header("Variables")] public float CurrentLootWeight;
    [SerializeField] public float MaxLootWeight;

    public List<GameFurniture> ListOfCollectedLoot = new List<GameFurniture>();
    public event Action CollectedLootChanged;

    private readonly List<float> _collectedLootWeights = new();

    public void Awake()
    {
        if (raid_References == null)
        {
            raid_References = Raid_References.Instance;
        }

        if (exitRaid == null)
        {
            exitRaid = ExitRaid.Instance;
        }
    }

    public void SetRandomMaxLootWeightRPC(float maxLootWeight)
    {
        MaxLootWeight = maxLootWeight;
        UpdateHeartLootText();
    }

    public void ResetLootCount()
    {
        ListOfCollectedLoot.Clear();
        _collectedLootWeights.Clear();
        CurrentLootWeight = 0;
        UpdateHeartLootText();
        CollectedLootChanged?.Invoke();
    }

    public void SetLootCount(GameFurniture furniture, float lootWeightMultiplier = 1f)
    {
        if (furniture == null)
        {
            return;
        }

        float addedLootWeight = (float)furniture.Weight * lootWeightMultiplier;
        ListOfCollectedLoot.Add(furniture);
        _collectedLootWeights.Add(addedLootWeight);

        CurrentLootWeight += addedLootWeight;

        UpdateHeartLootText();
        CollectedLootChanged?.Invoke();
        if (CurrentLootWeight > MaxLootWeight)
        {
            TriggerOverWeightEnd(MaxLootWeight);
        }
    }

    public bool RemoveCollectedLootAt(int lootIndex)
    {
        if (lootIndex < 0 || lootIndex >= ListOfCollectedLoot.Count)
        {
            return false;
        }

        GameFurniture removedFurniture = ListOfCollectedLoot[lootIndex];
        float removedWeight = GetCollectedLootWeight(lootIndex, removedFurniture);
        ListOfCollectedLoot.RemoveAt(lootIndex);
        if (lootIndex < _collectedLootWeights.Count)
        {
            _collectedLootWeights.RemoveAt(lootIndex);
        }

        CurrentLootWeight = Mathf.Max(0f, CurrentLootWeight - removedWeight);
        UpdateHeartLootText();
        CollectedLootChanged?.Invoke();
        return true;
    }

    private void UpdateHeartLootText()
    {
        if (HeartLootText != null)
        {
            HeartLootText.text = $"{CurrentLootWeight:F0}kg\n/{MaxLootWeight:F0}kg";
        }
    }

    private float GetCollectedLootWeight(int lootIndex, GameFurniture fallbackFurniture)
    {
        return lootIndex >= 0 && lootIndex < _collectedLootWeights.Count
            ? _collectedLootWeights[lootIndex]
            : GetFurnitureWeight(fallbackFurniture);
    }

    private static float GetFurnitureWeight(GameFurniture furniture)
    {
        return furniture != null ? (float)furniture.Weight : 0f;
    }

    private void TriggerOverWeightEnd(float maxLootWeight)
    {
        Image heartImage = raid_References != null
            && raid_References.Heart != null
            && raid_References.Heart.TryGetComponent(out Image image)
            ? image
            : null;

        if (heartImage != null)
        {
            if (BrokenHeartSprite != null)
            {
                heartImage.sprite = BrokenHeartSprite;
            }

            heartImage.enabled = true;
        }

        if (exitRaid != null)
        {
            exitRaid.EndRaid(ExitRaid.RaidEndReason.OutOfSpace);
        }
        else
        {
            Raid_EndMenu endMenu = raid_References != null
                ? raid_References.EndMenuController
                : null;
            if (endMenu != null)
            {
                endMenu.SetEndReasonText(ExitRaid.RaidEndReason.OutOfSpace);
                endMenu.SetLossHaloVisible(true);
                endMenu.SetOverWeightLimitBackground(true);
                endMenu.SetSpaceRemainingText(CurrentLootWeight, maxLootWeight);
            }

            raid_References?.ShowEndMenu();
        }
    }
    
}
