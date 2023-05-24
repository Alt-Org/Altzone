using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Raid_LootTracking : MonoBehaviour
{
    [SerializeField, Header("Reference scripts")] private Raid_References raid_References;

    [SerializeField, Header("Reference game components")] private TMP_Text CurrentLootText;
    [SerializeField] private TMP_Text OutOfText;
    [SerializeField] private TMP_Text MaxLootText;

    [SerializeField, Header("Variables")] public int CurrentLootWeight;
    [SerializeField] public int MaxLootWeight;

    public void Awake()
    {
        ResetLootCount();
    }

    public void ResetLootCount()
    {
        CurrentLootWeight = 0;
        this.CurrentLootText.text = CurrentLootWeight.ToString() + " kg";
        this.OutOfText.text = "Out of";
        this.MaxLootText.text =  MaxLootWeight.ToString() + " kg";
    }

    public void SetLootCount(int AddedLootWeight, int MaxLootWeight)
    {
        int NewLootWeight = CurrentLootWeight + AddedLootWeight;
        CurrentLootWeight = NewLootWeight;
        this.CurrentLootText.text = NewLootWeight.ToString() + " kg";
        this.MaxLootText.text = MaxLootWeight.ToString() + " kg";
        if(CurrentLootWeight > MaxLootWeight)
        {
            raid_References.RedScreen.SetActive(true);
            raid_References.EndMenu.SetActive(true);
            raid_References.OutOfSpace.enabled = true;
        }
    }
}
