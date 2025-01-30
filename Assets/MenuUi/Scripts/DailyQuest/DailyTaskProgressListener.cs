using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public class DailyTaskProgressListener : MonoBehaviour
{
    [SerializeField] private TaskType taskType = TaskType.Undefined;

    private void Awake()
    {
        enabled = false;
    }

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

    //Call this function from location where its corresponding task will be seen as valid progress.
    public void UpdateProgress(string value)
    {
        DailyTaskProgressManager.Instance.UpdateTaskProgress(taskType, value);
    }

    public void SetState(TaskType currentTaskType)
    {
        enabled = (taskType == currentTaskType);
    }
}
