using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public class DailyTaskProgressListener : MonoBehaviour
{
    [SerializeField] private TaskType taskType = TaskType.Undefined;
    private bool _on = false;

    private void Start()
    {
        try
        {
            DailyTaskProgressManager.OnTaskChange += SetState;
            _on = DailyTaskProgressManager.Instance.SameTask(taskType);
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
            DailyTaskProgressManager.OnTaskChange -= SetState;
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    /// <summary>
    /// Call this function from location where it's corresponding<br/>
    /// task will be seen as valid daily task progress.<br/><br/>
    /// Normal use case: give an integer value of 1 or greater as a string.<br/><br/>
    /// Special use case: give a character name or other unique identifier as a string<br/>
    /// (eg. Start 3 battles with different characters).
    /// </summary>
    public void UpdateProgress(string value)
    {
        try
        {
            if (_on)
                DailyTaskProgressManager.Instance.UpdateTaskProgress(taskType, value);
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    public void SetState(TaskType currentTaskType)
    {
        //-----TEST CODE-----
        if (taskType == TaskType.Test)
        {
            _on = true;
            return;
        }
        //-------------------

        _on = (taskType == currentTaskType);
    }
}
