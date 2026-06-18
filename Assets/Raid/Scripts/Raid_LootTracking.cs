using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Altzone.Scripts.Model.Poco.Game;
using System;
//using Photon.Pun;

public class Raid_LootTracking : MonoBehaviour//PunCallbacks
{
    [SerializeField, Header("Reference scripts")] private Raid_References raid_References;
    [SerializeField] private ExitRaid exitRaid;

    [SerializeField, Header("Reference game components")] private TMP_Text HeartLootText;
    [SerializeField] private Sprite BrokenHeartSprite;
    
    [SerializeField, Header("Variables")] public float CurrentLootWeight;
    [SerializeField] public float MaxLootWeight;

    //public PhotonView _photonView { get; private set; }
    public List<GameFurniture> ListOfCollectedLoot = new List<GameFurniture>();
    public event Action CollectedLootChanged;

    private readonly List<float> _collectedLootWeights = new();
    private readonly Dictionary<string, float> _clanCurrentWeights = new();
    private readonly Dictionary<string, float> _clanMaxWeights = new();
    private readonly Dictionary<string, List<GameFurniture>> _clanCollectedLoot = new();
    private readonly Dictionary<string, List<float>> _clanCollectedLootWeights = new();
    private string _displayedClanId = string.Empty;

    public void Awake()
    {
        if (exitRaid == null)
        {
            exitRaid = ExitRaid.Instance;
        }

/*        _photonView = gameObject.AddComponent<PhotonView>();
        _photonView.ViewID = 3;
        if (PhotonNetwork.IsMasterClient)
        {
            float randomlootWeight = Random.Range(140, 241);
            _photonView.RPC(nameof(SetRandomMaxLootWeightRPC), RpcTarget.AllBuffered, randomlootWeight);
        }

        ResetLootCount();*/
    }

    /*[PunRPC]*/
    public void SetRandomMaxLootWeightRPC(float maxLootWeight)
    {
        MaxLootWeight = maxLootWeight;
        //Debug.Log("maxLootWeight has been set");
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
        _clanCurrentWeights.Clear();
        _clanMaxWeights.Clear();
        _clanCollectedLoot.Clear();
        _clanCollectedLootWeights.Clear();
        _collectedLootWeights.Clear();
        ListOfCollectedLoot = new List<GameFurniture>();
        CurrentLootWeight = 0;
        UpdateHeartLootText();
        CollectedLootChanged?.Invoke();
    }

    public void SetDisplayedClan(string clanId)
    {
        _displayedClanId = clanId ?? string.Empty;
        EnsureClanState(_displayedClanId, MaxLootWeight);
        RefreshDisplayedClanValues();
    }

    public void SetClanLimit(string clanId, float maxLootWeight)
    {
        if (string.IsNullOrWhiteSpace(clanId))
        {
            return;
        }

        EnsureClanState(clanId, maxLootWeight);
        _clanMaxWeights[clanId] = maxLootWeight;

        if (clanId == _displayedClanId)
        {
            RefreshDisplayedClanValues();
        }
    }

    public void SetLootCount(GameFurniture furniture, float MaxLootWeight, float lootWeightMultiplier = 1f)
    {
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
        if (string.IsNullOrWhiteSpace(clanId) || furniture == null)
        {
            return;
        }

        float configuredMaxWeight = ResolveClanMaxWeight(clanId, maxLootWeight);
        EnsureClanState(clanId, configuredMaxWeight);

        float addedLootWeight = (float)furniture.Weight * lootWeightMultiplier;
        _clanCollectedLoot[clanId].Add(furniture);
        _clanCollectedLootWeights[clanId].Add(addedLootWeight);
        _clanCurrentWeights[clanId] += addedLootWeight;

        if (clanId != _displayedClanId)
        {
            return;
        }

        RefreshDisplayedClanValues();
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
        if (string.IsNullOrWhiteSpace(clanId))
        {
            return false;
        }

        EnsureClanState(clanId, MaxLootWeight);
        List<GameFurniture> clanLoot = _clanCollectedLoot[clanId];
        if (lootIndex < 0 || lootIndex >= clanLoot.Count)
        {
            return false;
        }

        GameFurniture removedFurniture = clanLoot[lootIndex];
        float removedWeight = GetClanCollectedLootWeight(clanId, lootIndex, removedFurniture);
        clanLoot.RemoveAt(lootIndex);
        if (_clanCollectedLootWeights.TryGetValue(clanId, out List<float> clanWeights) && lootIndex < clanWeights.Count)
        {
            clanWeights.RemoveAt(lootIndex);
        }

        _clanCurrentWeights[clanId] = Mathf.Max(0f, _clanCurrentWeights[clanId] - removedWeight);

        if (clanId == _displayedClanId)
        {
            RefreshDisplayedClanValues();
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

    private float ResolveClanMaxWeight(string clanId, float fallbackMaxWeight)
    {
        if (_clanMaxWeights.TryGetValue(clanId, out float configuredMaxWeight) && configuredMaxWeight > 0f)
        {
            return configuredMaxWeight;
        }

        if (fallbackMaxWeight > 0f)
        {
            return fallbackMaxWeight;
        }

        return MaxLootWeight;
    }

    private void EnsureClanState(string clanId, float maxLootWeight)
    {
        if (string.IsNullOrWhiteSpace(clanId))
        {
            return;
        }

        if (!_clanCurrentWeights.ContainsKey(clanId))
        {
            _clanCurrentWeights[clanId] = 0f;
        }

        if (!_clanMaxWeights.ContainsKey(clanId))
        {
            _clanMaxWeights[clanId] = maxLootWeight;
        }

        if (!_clanCollectedLoot.ContainsKey(clanId))
        {
            _clanCollectedLoot[clanId] = new List<GameFurniture>();
        }

        if (!_clanCollectedLootWeights.ContainsKey(clanId))
        {
            _clanCollectedLootWeights[clanId] = new List<float>();
        }
    }

    private void RefreshDisplayedClanValues()
    {
        if (string.IsNullOrWhiteSpace(_displayedClanId))
        {
            return;
        }

        EnsureClanState(_displayedClanId, MaxLootWeight);
        CurrentLootWeight = _clanCurrentWeights[_displayedClanId];
        MaxLootWeight = _clanMaxWeights[_displayedClanId];
        ListOfCollectedLoot = _clanCollectedLoot[_displayedClanId];
        _collectedLootWeights.Clear();
        _collectedLootWeights.AddRange(_clanCollectedLootWeights[_displayedClanId]);
        UpdateHeartLootText();
    }

    private float GetCollectedLootWeight(int lootIndex, GameFurniture fallbackFurniture)
    {
        return lootIndex >= 0 && lootIndex < _collectedLootWeights.Count
            ? _collectedLootWeights[lootIndex]
            : GetFurnitureWeight(fallbackFurniture);
    }

    private float GetClanCollectedLootWeight(string clanId, int lootIndex, GameFurniture fallbackFurniture)
    {
        return _clanCollectedLootWeights.TryGetValue(clanId, out List<float> clanWeights)
            && lootIndex >= 0
            && lootIndex < clanWeights.Count
                ? clanWeights[lootIndex]
                : GetFurnitureWeight(fallbackFurniture);
    }

    private static float GetFurnitureWeight(GameFurniture furniture)
    {
        return furniture != null ? (float)furniture.Weight : 0f;
    }

    private void TriggerOverWeightEnd(float maxLootWeight)
    {
        Image heartImage = raid_References != null && raid_References.Heart != null
            ? raid_References.Heart.GetComponent<Image>()
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
            Raid_EndMenu endMenu = raid_References.EndMenu.GetComponent<Raid_EndMenu>();
            if (endMenu != null)
            {
                endMenu.SetEndReasonText(ExitRaid.RaidEndReason.OutOfSpace);
                endMenu.SetLossHaloVisible(true);
                endMenu.SetOverWeightLimitBackground(true);
                endMenu.SetSpaceRemainingText(CurrentLootWeight, maxLootWeight);
            }

            raid_References.EndMenu.SetActive(true);
        }
    }
    
}
