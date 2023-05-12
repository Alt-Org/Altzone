using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Raid_LootTracking : MonoBehaviour
{
    [SerializeField] private TMP_Text CurrentLootText;
    [SerializeField] private TMP_Text OutOfText;
    [SerializeField] private TMP_Text MaxLootText;
    [SerializeField] private int CurrentLootWeight;

    public void Awake()
    {
        ResetLootCount();
    }

    public void ResetLootCount()
    {
        CurrentLootWeight = 0;
        this.CurrentLootText.text = "0 kg";
        this.OutOfText.text = "Out of";
        this.MaxLootText.text = "250 kg";
    }

    public void SetLootCount(int AddedLootWeight, int MaxLootWeight)
    {
        int NewLootWeight = CurrentLootWeight + AddedLootWeight;
        CurrentLootWeight = NewLootWeight;
        this.CurrentLootText.text = NewLootWeight.ToString() + " kg";
        this.MaxLootText.text = MaxLootWeight.ToString() + " kg";
    }
}
