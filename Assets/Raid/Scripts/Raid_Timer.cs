using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Raid_Timer : MonoBehaviour
{
    [SerializeField, Header("Loot manager")]
    private Raid_LootManagement raid_LootManagement;

    [SerializeField, Header("Raid Inventory ref")]
    private Raid_Inventory raid_Inventory;

    [Header("Timer text")]
    public TextMeshProUGUI TimerText;

    [Header("Timer settings")]
    public float CurrentTime;
    public bool CountUp;

    [Header("Limit settings")]
    public bool HasLimit;
    public float TimerLimit;

    [Header("Format settings")]
    public bool HasFormat;
    public TimerFormat Format;
    private Dictionary<TimerFormat, string> TimeFormat = new Dictionary<TimerFormat, string>();

    void Start()
    {
        TimeFormat.Add(TimerFormat.Whole, "0");
        TimeFormat.Add(TimerFormat.TenthDecimal, "0.0");
        TimeFormat.Add(TimerFormat.HundrethsDecimal, "0.00");
    }

    void Update()
    {
        CurrentTime = CountUp ? CurrentTime += Time.deltaTime : CurrentTime -= Time.deltaTime;

        if (HasLimit && ((CountUp && CurrentTime >= TimerLimit) || (!CountUp && CurrentTime <= TimerLimit)))
        {
            raid_Inventory.RedScreen.SetActive(true);
            raid_Inventory.EndMenu.SetActive(true);
            CurrentTime = TimerLimit;
            SetTimerText();
            TimerText.color = Color.red;
            enabled = false;
            if(raid_LootManagement.CurrentLootWeight <= raid_LootManagement.WeightLimit)
            {
                raid_LootManagement.LootWeightText.color = Color.green;
            }
            else if (raid_LootManagement.CurrentLootWeight > raid_LootManagement.WeightLimit)
            {
                raid_LootManagement.LootWeightText.color = Color.red;
            }
        }
        SetTimerText();
    }

    private void SetTimerText()
    {
        TimerText.text = HasFormat ? CurrentTime.ToString(TimeFormat[Format]) : CurrentTime.ToString();
    }

    public enum TimerFormat
    {
        Whole, TenthDecimal, HundrethsDecimal
    }
}
