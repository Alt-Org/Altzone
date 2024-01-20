using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartScript : MonoBehaviour
{
    public Raid_LootTracking raid_LootTracking;
    private float amount;
    public Color white = Color.white;
    public Color black = Color.black;

    public void UpdateColor()
    {
        amount = raid_LootTracking.CurrentLootWeight / raid_LootTracking.MaxLootWeight;
        Color mix = Color.Lerp(white, black, amount);
        this.GetComponent<Image>().color = mix;

    }
}
