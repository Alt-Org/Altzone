using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public class DailyTaskProgressListener : MonoBehaviour
{
    [SerializeField] private TaskType taskType = TaskType.Undefined;

    private void Start()
    {
        try
        {
            DailyTaskProgressManager.Instance.OnTaskChange += SetState;
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }

        enabled = false;
    }

    private void OnDestroy()
    {
        try
        {
            DailyTaskProgressManager.Instance.OnTaskChange -= SetState;
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    /*
     *Call this function from location where it's corresponding task will be seen as valid daily task progress.
     *Normal use case: give a integer value of 1 or greater as a string.
     *Special use case: give a character name or other unique identifier as a string (eg. Start 3 battles with different characters).
     */
    public void UpdateProgress(string value)
    {
        try
        {
            DailyTaskProgressManager.Instance.UpdateTaskProgress(taskType, value);
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    public void SetState(TaskType currentTaskType)
    {
        enabled = (taskType == currentTaskType);
    }
}
