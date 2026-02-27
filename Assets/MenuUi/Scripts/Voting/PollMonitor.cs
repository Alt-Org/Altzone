using UnityEngine;
using System.Collections;

public class PollMonitor : MonoBehaviour // Monitors active polls to check if they should be expired
{
    public static PollMonitor Instance { get; private set; }

    private Coroutine checkRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        ServerManager.OnClanPollsChanged += BuildPolls;
    }

    private void OnDestroy()
    {
        ServerManager.OnClanPollsChanged -= BuildPolls;
    }

    private void BuildPolls() => PollManager.BuildPolls();

    // Start monitoring when a poll begins
    public void StartMonitoring()
    {
        if (checkRoutine == null)
        {
            checkRoutine = StartCoroutine(CheckExpiredPollsRoutine());
            Debug.Log("Start Monitoring");
            PollManager.DebugPrintAllActivePolls();
        }
    }

    // Stop monitoring when there are no polls left
    public void StopMonitoring()
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
            Debug.Log("Stop Monitoring");
        }
    }

    // Expire polls that have run out, checking them every 5 seconds. If no polls are active, stop the coroutine for checking and stop monitoring
    private IEnumerator CheckExpiredPollsRoutine()
    {
        while (true)
        {
            PollManager.CheckAndExpirePolls();

            if (PollManager.GetPollList().Count == 0)
            {
                StopMonitoring();
                yield break;
            }

            Debug.Log("[PollMonitor] Checking for expired polls");
            yield return new WaitForSeconds(5f);
        }
    }
}
