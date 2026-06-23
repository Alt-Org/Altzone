using System;
using System.Collections;
using UnityEngine;

public class ExitRaid : MonoBehaviour
{
    public static ExitRaid Instance { get; private set; }

    public enum RaidEndReason
    {
        Trap,
        OutOfTime,
        OutOfSpace,
        PlayerExit
    }

    public event Action ExitedRaid;
    public bool raidEnded = false;
    [SerializeField] private Raid_Timer raid_timer;
    [SerializeField, Header("Reference scripts")] private Raid_References raid_References;
    [SerializeField] private Raid_EventLog eventLog;

    private void Awake()
    {
        RegisterInstance();
        ResolveReferences();
    }

    private void OnEnable()
    {
        RegisterInstance();
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RequestExitRaid()
    {
        if (Instance != null && Instance != this)
        {
            Instance.RequestExitRaid();
            return;
        }

        if (raidEnded)
        {
            return;
        }

        Raid_ExitConfirmationPopup.Show(() => EndRaid(RaidEndReason.PlayerExit));
    }

    public void EndRaid()
    {
        EndRaid(RaidEndReason.Trap);
    }

    public void EndRaid(RaidEndReason reason)
    {
        if (Instance != null && Instance != this)
        {
            Instance.EndRaid(reason);
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
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
        eventLog = eventLog != null ? eventLog : Raid_EventLog.FindForInventory(transform);
        eventLog?.LogSystemMessage("Peli p\u00E4\u00E4ttyi", "Game ended");

        SetLossHaloVisuals(reason);
        OnEndRaid();
        raid_timer.FinishRaid();

        StartCoroutine(EndRaidRoutine(reason));
    }

    private IEnumerator EndRaidRoutine(RaidEndReason reason)
    {
        SetEndReasonText(reason);

        yield return Raid_EventPopup.ShowAndWait(this, GetPopupScenario(reason));

        Raid_EndMenu endMenu = raid_References.EndMenuController;
        if (endMenu == null || raid_References.raid_LootTracking == null)
        {
            Debug.LogError("Cannot show raid end menu because the end menu or loot tracking reference is missing.");
            yield break;
        }

        endMenu.SetLossHaloVisible(reason == RaidEndReason.OutOfSpace);
        endMenu.SetOverWeightLimitBackground(reason == RaidEndReason.OutOfSpace);
        endMenu.SetCollectedLoot(raid_References.raid_LootTracking.ListOfCollectedLoot);
        endMenu.SetSpaceRemainingText(raid_References.raid_LootTracking.CurrentLootWeight, raid_References.raid_LootTracking.MaxLootWeight);
        raid_References.ShowEndMenu();
    }

    public void OnEndRaid()
    {
        ExitedRaid?.Invoke();
    }

    private void SetEndReasonText(RaidEndReason reason)
    {
        Raid_EndMenu endMenu = raid_References.EndMenuController;
        if (endMenu != null)
        {
            endMenu.SetEndReasonText(reason);
        }
    }

    private void SetLossHaloVisuals(RaidEndReason reason)
    {
        HeartScript heartScript = raid_References != null
            && raid_References.Heart != null
            && raid_References.Heart.TryGetComponent(out HeartScript referencedHeart)
            ? referencedHeart
            : null;

        if (heartScript == null)
        {
            heartScript = FindObjectOfType<HeartScript>();
        }

        if (heartScript != null)
        {
            heartScript.SetLossHaloVisible(reason == RaidEndReason.OutOfSpace);
        }

        if (raid_timer != null)
        {
            raid_timer.SetLossHaloVisible(true);
        }
    }

    private void RegisterInstance()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
            return;
        }

        if (!Instance.gameObject.activeInHierarchy && gameObject.activeInHierarchy)
        {
            Instance = this;
            return;
        }

        Debug.LogWarning("Multiple ExitRaid instances found. Using the first active instance.");
    }

    private Raid_EventPopup.Scenario GetPopupScenario(RaidEndReason reason)
    {
        switch (reason)
        {
            case RaidEndReason.OutOfTime:
                return Raid_EventPopup.Scenario.OutOfTime;
            case RaidEndReason.OutOfSpace:
                return Raid_EventPopup.Scenario.OutOfSpace;
            case RaidEndReason.PlayerExit:
                return Raid_EventPopup.Scenario.PlayerExit;
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

        if (eventLog == null)
        {
            eventLog = Raid_EventLog.FindForInventory(transform);
        }
    }
}
