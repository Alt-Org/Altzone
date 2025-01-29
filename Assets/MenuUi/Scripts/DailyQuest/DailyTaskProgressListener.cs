using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public class DailyTaskProgressListener : MonoBehaviour
{

    [SerializeField] private TaskType taskType = TaskType.Undefined;

    private void Start()
    {
        DailyTaskProgressManager.Instance.AddListener(this);
    }

    public void UpdateProgress(string value)
    {
        DailyTaskProgressManager.Instance.UpdateTaskProgress();
    }
}
