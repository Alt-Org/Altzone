using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Raid_LootManagement : MonoBehaviour
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
        SetLootWeightText();
    }

    public void SetLootWeightText()
    {
        LootWeightText.text = CurrentLootWeight.ToString() + "/" + WeightLimit.ToString() + "kg";
    }
}
