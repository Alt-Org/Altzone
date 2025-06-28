using UnityEngine;
using System.Collections;
public class PollMonitor : MonoBehaviour
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
    }

    public void StartMonitoring()
    {
        if (checkRoutine == null)
        {
            checkRoutine = StartCoroutine(CheckExpiredPollsRoutine());
            Debug.Log("Start Monitoring");
        }
    }

    public void StopMonitoring()
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
            Debug.Log("Stop Monitoring");
        }
    }

    private IEnumerator CheckExpiredPollsRoutine()
    {
        while (true)
        {
            PollManager.CheckAndExpiredPolls();

            if (PollManager.GetPollList().Count == 0)
            {
                StopMonitoring();
                yield break;
            }

            yield return new WaitForSeconds(5f);
        }
    }
}
