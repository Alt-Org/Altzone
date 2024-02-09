using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExitRaid : MonoBehaviour
{

    public event Action ExitedRaid;
    public bool raidEnded = false;
    [SerializeField] private Raid_Timer raid_timer;
    [SerializeField, Header("Reference scripts")] private Raid_References raid_References;

    public void EndRaid()
    {
        raidEnded = true;
        OnEndRaid();
        raid_References.RedScreen.SetActive(true);
        raid_References.EndMenu.SetActive(true);

        if(!raid_References.OutOfSpace.enabled && !raid_References.OutOfTime.enabled)
            raid_References.RaidEndedText.enabled = true;

        raid_timer.FinishRaid();
    }
    public void OnEndRaid()
    {
        ExitedRaid?.Invoke();
    }
}

