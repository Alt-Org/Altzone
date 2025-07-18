using System.Collections.Generic;
using UnityEngine;

public class DailyTaskData
{
    public string title;
    public int amount;
    public int coins;
    public int points;
}

[System.Serializable]
public class NormalDailyTaskData : DailyTaskData
{
    public string type;
}

[System.Serializable]
public class EducationDailyTaskData : DailyTaskData
{
    public string educationCategoryType;
    public string educationCategoryTaskType;
}

//[CreateAssetMenu(fileName = "DailyTaskConfig")]
public class DailyTaskConfig : ScriptableObject
{
    private static DailyTaskConfig _instance;

    public static DailyTaskConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<DailyTaskConfig>(nameof(DailyTaskConfig));
            }

            return _instance;
        }
    }

    [SerializeField] private List<NormalDailyTaskData> _normalDailyTasks;
    [SerializeField] private List<EducationDailyTaskData> _educationDailyTasks;

    public List<NormalDailyTaskData> GetNormalTasks() => _normalDailyTasks;
    public List<EducationDailyTaskData> GetEducationTasks() => _educationDailyTasks;
}
