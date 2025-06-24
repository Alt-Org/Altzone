using UnityEngine;
using System.Collections;

public class PollMonitor : MonoBehaviour // Handles monitoring the polls and expires the polls artificially by checking if their timer would've already run out
{
    private void Start()
    {
        StartCoroutine(CheckExpiredPollsRoutine());
    }

    private IEnumerator CheckExpiredPollsRoutine()
    {
        while (true)
        {
            PollManager.CheckAndExpiredPolls();  
            yield return new WaitForSeconds(5f); 
        }
    }
}
