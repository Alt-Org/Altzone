using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Altzone.Scripts.Model.Poco.Game;
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
    private readonly Dictionary<string, float> _clanCurrentWeights = new();
    private readonly Dictionary<string, float> _clanMaxWeights = new();
    private readonly Dictionary<string, List<GameFurniture>> _clanCollectedLoot = new();
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
        CurrentLootWeight = 0;
        UpdateHeartLootText();
    }

    public void ResetClanLootCounts()
    {
        _clanCurrentWeights.Clear();
        _clanMaxWeights.Clear();
        _clanCollectedLoot.Clear();
        ListOfCollectedLoot = new List<GameFurniture>();
        CurrentLootWeight = 0;
        UpdateHeartLootText();
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

        float NewLootWeight = CurrentLootWeight + AddedLootWeight;
        CurrentLootWeight = NewLootWeight;

        UpdateHeartLootText();
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
        _clanCurrentWeights[clanId] += addedLootWeight;

        if (clanId != _displayedClanId)
        {
            return;
        }

        RefreshDisplayedClanValues();
        if (CurrentLootWeight > MaxLootWeight)
        {
            TriggerOverWeightEnd(MaxLootWeight);
        }
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
        UpdateHeartLootText();
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
