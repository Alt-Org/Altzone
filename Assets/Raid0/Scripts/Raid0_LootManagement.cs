using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Raid0_LootManagement : MonoBehaviour
{
    [Header("Component")]
    public TextMeshProUGUI LootWeightText;

    [Header("Limit settings")]
    public bool HasWeightLimit;
    public float WeightLimit = 100f;

    [Header("Weight settings")]
    public float CurrentLootWeight = 0f;

    private void Start()
    {
        CurrentLootWeight = 0f;
        SetLootWeightText();
    }

    public void SetLootWeightText()
    {
        LootWeightText.text = CurrentLootWeight.ToString() + "/" + WeightLimit.ToString() + "kg";
    }
}
