using System;
using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        ResolveReferences();
    }

    public void EndRaid()
    {
        EndRaid(RaidEndReason.Trap);
    }

    public void EndRaid(RaidEndReason reason)
    {
        if (!gameObject.activeInHierarchy)
        {
            ExitRaid activeExitRaid = FindActiveExitRaid();
            if (activeExitRaid != null && activeExitRaid != this)
            {
                activeExitRaid.EndRaid(reason);
                return;
            }

            Debug.LogError("Cannot end raid from an inactive ExitRaid object.");
            return;
        }

        if (raidEnded)
        {
            return;
        }

        ResolveReferences();
        if (raid_timer == null || raid_References == null)
        {
            Debug.LogError("Cannot end raid because ExitRaid is missing raid references.");
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

        Raid_EndMenu endMenu = raid_References.EndMenu == null ? null : raid_References.EndMenu.GetComponent<Raid_EndMenu>();
        if (endMenu == null)
        {
            Debug.LogError("Cannot show raid end menu because the EndMenu reference is missing Raid_EndMenu.");
            yield break;
        }

        endMenu.SetOverWeightLimitBackground(reason == RaidEndReason.OutOfSpace);
        endMenu.SetCollectedLoot(raid_References.raid_LootTracking.ListOfCollectedLoot);
        raid_References.EndMenu.SetActive(true);
    }

    public void OnEndRaid()
    {
        ExitedRaid?.Invoke();
    }

    private void SetEndReasonText(RaidEndReason reason)
    {
        Raid_EndMenu endMenu = raid_References.EndMenu == null ? null : raid_References.EndMenu.GetComponent<Raid_EndMenu>();
        if (endMenu != null)
        {
            endMenu.SetEndReasonText(reason);
        }
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

    private void ResolveReferences()
    {
        if (raid_timer == null)
        {
            raid_timer = FindObjectOfType<Raid_Timer>();
        }

        if (raid_References == null)
        {
            raid_References = FindObjectOfType<Raid_References>();
        }
    }

    private ExitRaid FindActiveExitRaid()
    {
        ExitRaid[] exitRaids = FindObjectsOfType<ExitRaid>();
        foreach (ExitRaid exitRaid in exitRaids)
        {
            if (exitRaid != null && exitRaid.gameObject.activeInHierarchy)
            {
                return exitRaid;
            }
        }

        return null;
    }
}
