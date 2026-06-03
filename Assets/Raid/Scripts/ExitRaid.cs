using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExitRaid : MonoBehaviour
{
    public enum RaidEndReason
    {
        Trap,
        OutOfTime,
        OutOfSpace
    }

    public event Action ExitedRaid;
    public bool raidEnded = false;
    [SerializeField] private Raid_Timer raid_timer;
    [SerializeField, Header("Reference scripts")] private Raid_References raid_References;

    public void EndRaid()
    {
        EndRaid(RaidEndReason.Trap);
    }

    public void EndRaid(RaidEndReason reason)
    {
        if (raidEnded)
        {
            return;
        }

        raidEnded = true;
        OnEndRaid();
        raid_timer.FinishRaid();

        StartCoroutine(EndRaidRoutine(reason));
    }

    private IEnumerator EndRaidRoutine(RaidEndReason reason)
    {
        SetEndReasonText(reason);

        yield return Raid_EventPopup.ShowAndWait(this, GetPopupScenario(reason));

        raid_References.EndMenu.GetComponent<Raid_EndMenu>().SetCollectedLoot(raid_References.raid_LootTracking.ListOfCollectedLoot);
        raid_References.EndMenu.SetActive(true);
    }

    public void OnEndRaid()
    {
        ExitedRaid?.Invoke();
    }

    private void SetEndReasonText(RaidEndReason reason)
    {
        raid_References.OutOfTime.enabled = reason == RaidEndReason.OutOfTime;
        raid_References.OutOfSpace.enabled = reason == RaidEndReason.OutOfSpace;
        raid_References.RaidEndedText.enabled = reason == RaidEndReason.Trap;
    }

    private Raid_EventPopup.Scenario GetPopupScenario(RaidEndReason reason)
    {
        switch (reason)
        {
            case RaidEndReason.OutOfTime:
                return Raid_EventPopup.Scenario.OutOfTime;
            case RaidEndReason.OutOfSpace:
                return Raid_EventPopup.Scenario.OutOfSpace;
            default:
                return Raid_EventPopup.Scenario.EndTrap;
        }
    }
}
