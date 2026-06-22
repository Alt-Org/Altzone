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
    private readonly Dictionary<string, float> _ownerCurrentWeights = new();
    private readonly Dictionary<string, float> _ownerMaxWeights = new();
    private readonly Dictionary<string, List<GameFurniture>> _ownerCollectedLoot = new();
    private readonly Dictionary<string, List<float>> _ownerCollectedLootWeights = new();
    private string _displayedOwnerId = string.Empty;

    public void Awake()
    {
        if (raid_References == null)
        {
            Raid_References[] references = FindObjectsOfType<Raid_References>(true);
            raid_References = references.Length > 0 ? references[0] : null;
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
        ListOfCollectedLoot = new List<GameFurniture>();
        _collectedLootWeights.Clear();
        CurrentLootWeight = 0;
        UpdateHeartLootText();
        CollectedLootChanged?.Invoke();
    }

    public void ResetClanLootCounts()
    {
        ResetLootOwnerCounts();
    }

    public void ResetLootOwnerCounts()
    {
        _ownerCurrentWeights.Clear();
        _ownerMaxWeights.Clear();
        _ownerCollectedLoot.Clear();
        _ownerCollectedLootWeights.Clear();
        _collectedLootWeights.Clear();
        ListOfCollectedLoot = new List<GameFurniture>();
        CurrentLootWeight = 0;
        UpdateHeartLootText();
        CollectedLootChanged?.Invoke();
    }

    public void SetDisplayedClan(string clanId)
    {
        SetDisplayedLootOwner(clanId);
    }

    public void SetDisplayedLootOwner(string ownerId)
    {
        _displayedOwnerId = ownerId ?? string.Empty;
        EnsureLootOwnerState(_displayedOwnerId, MaxLootWeight);
        RefreshDisplayedOwnerValues();
    }

    public bool IsDisplayedClan(string clanId)
    {
        return IsDisplayedLootOwner(clanId);
    }

    public bool IsDisplayedLootOwner(string ownerId)
    {
        return !string.IsNullOrWhiteSpace(ownerId) && ownerId == _displayedOwnerId;
    }

    public void SetClanLimit(string clanId, float maxLootWeight)
    {
        SetLootOwnerLimit(clanId, maxLootWeight);
    }

    public void SetLootOwnerLimit(string ownerId, float maxLootWeight)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return;
        }

        EnsureLootOwnerState(ownerId, maxLootWeight);
        _ownerMaxWeights[ownerId] = maxLootWeight;

        if (ownerId == _displayedOwnerId)
        {
            RefreshDisplayedOwnerValues();
        }
    }

    public void SetLootCount(GameFurniture furniture, float MaxLootWeight, float lootWeightMultiplier = 1f)
    {
        if (furniture == null)
        {
            return;
        }

        float AddedLootWeight = (float)furniture.Weight * lootWeightMultiplier;
        ListOfCollectedLoot.Add(furniture);
        _collectedLootWeights.Add(AddedLootWeight);

        float NewLootWeight = CurrentLootWeight + AddedLootWeight;
        CurrentLootWeight = NewLootWeight;

        UpdateHeartLootText();
        CollectedLootChanged?.Invoke();
        if (CurrentLootWeight > MaxLootWeight)
        {
            TriggerOverWeightEnd(MaxLootWeight);
        }
    }

    public void SetClanLootCount(string clanId, GameFurniture furniture, float maxLootWeight, float lootWeightMultiplier = 1f)
    {
        SetLootOwnerLootCount(clanId, furniture, maxLootWeight, lootWeightMultiplier);
    }

    public void SetLootOwnerLootCount(string ownerId, GameFurniture furniture, float maxLootWeight, float lootWeightMultiplier = 1f)
    {
        if (string.IsNullOrWhiteSpace(ownerId) || furniture == null)
        {
            return;
        }

        float configuredMaxWeight = ResolveLootOwnerMaxWeight(ownerId, maxLootWeight);
        EnsureLootOwnerState(ownerId, configuredMaxWeight);

        float addedLootWeight = (float)furniture.Weight * lootWeightMultiplier;
        _ownerCollectedLoot[ownerId].Add(furniture);
        _ownerCollectedLootWeights[ownerId].Add(addedLootWeight);
        _ownerCurrentWeights[ownerId] += addedLootWeight;

        if (ownerId != _displayedOwnerId)
        {
            return;
        }

        RefreshDisplayedOwnerValues();
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

    public bool RemoveClanCollectedLootAt(string clanId, int lootIndex)
    {
        return RemoveLootOwnerCollectedLootAt(clanId, lootIndex);
    }

    public bool RemoveLootOwnerCollectedLootAt(string ownerId, int lootIndex)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return false;
        }

        EnsureLootOwnerState(ownerId, MaxLootWeight);
        List<GameFurniture> ownerLoot = _ownerCollectedLoot[ownerId];
        if (lootIndex < 0 || lootIndex >= ownerLoot.Count)
        {
            return false;
        }

        GameFurniture removedFurniture = ownerLoot[lootIndex];
        float removedWeight = GetLootOwnerCollectedLootWeight(ownerId, lootIndex, removedFurniture);
        ownerLoot.RemoveAt(lootIndex);
        if (_ownerCollectedLootWeights.TryGetValue(ownerId, out List<float> ownerWeights) && lootIndex < ownerWeights.Count)
        {
            ownerWeights.RemoveAt(lootIndex);
        }

        _ownerCurrentWeights[ownerId] = Mathf.Max(0f, _ownerCurrentWeights[ownerId] - removedWeight);

        if (ownerId == _displayedOwnerId)
        {
            RefreshDisplayedOwnerValues();
        }

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

    private float ResolveLootOwnerMaxWeight(string ownerId, float fallbackMaxWeight)
    {
        if (_ownerMaxWeights.TryGetValue(ownerId, out float configuredMaxWeight) && configuredMaxWeight > 0f)
        {
            return configuredMaxWeight;
        }

        if (fallbackMaxWeight > 0f)
        {
            return fallbackMaxWeight;
        }

        return MaxLootWeight;
    }

    private void EnsureLootOwnerState(string ownerId, float maxLootWeight)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return;
        }

        if (!_ownerCurrentWeights.ContainsKey(ownerId))
        {
            _ownerCurrentWeights[ownerId] = 0f;
        }

        if (!_ownerMaxWeights.ContainsKey(ownerId))
        {
            _ownerMaxWeights[ownerId] = maxLootWeight;
        }

        if (!_ownerCollectedLoot.ContainsKey(ownerId))
        {
            _ownerCollectedLoot[ownerId] = new List<GameFurniture>();
        }

        if (!_ownerCollectedLootWeights.ContainsKey(ownerId))
        {
            _ownerCollectedLootWeights[ownerId] = new List<float>();
        }
    }

    private void RefreshDisplayedOwnerValues()
    {
        if (string.IsNullOrWhiteSpace(_displayedOwnerId))
        {
            return;
        }

        EnsureLootOwnerState(_displayedOwnerId, MaxLootWeight);
        CurrentLootWeight = _ownerCurrentWeights[_displayedOwnerId];
        MaxLootWeight = _ownerMaxWeights[_displayedOwnerId];
        ListOfCollectedLoot = _ownerCollectedLoot[_displayedOwnerId];
        _collectedLootWeights.Clear();
        _collectedLootWeights.AddRange(_ownerCollectedLootWeights[_displayedOwnerId]);
        UpdateHeartLootText();
    }

    private float GetCollectedLootWeight(int lootIndex, GameFurniture fallbackFurniture)
    {
        return lootIndex >= 0 && lootIndex < _collectedLootWeights.Count
            ? _collectedLootWeights[lootIndex]
            : GetFurnitureWeight(fallbackFurniture);
    }

    private float GetLootOwnerCollectedLootWeight(string ownerId, int lootIndex, GameFurniture fallbackFurniture)
    {
        return _ownerCollectedLootWeights.TryGetValue(ownerId, out List<float> ownerWeights)
            && lootIndex >= 0
            && lootIndex < ownerWeights.Count
                ? ownerWeights[lootIndex]
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
