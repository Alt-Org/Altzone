using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using static DailyQuest;

public class ChooseTask : MonoBehaviour
{

    [SerializeField] private RectTransform _taskCardHolder;

    private DailyTaskManager _dtManager;
    private VersionType _gameVersion;

    private void Start()
    {
        _dtManager = GetComponentInParent<DailyTaskManager>();
        _gameVersion = GameConfig.Get().GameVersionType;
    }


    private DailyQuest GetRandomQuest()
    {
        DailyTaskManager dtManager = GetComponentInParent<DailyTaskManager>();

        List<GameObject> taskCardSlots = dtManager.DailyTaskCardSlots;

        int randomListNumber = Random.Range(0, taskCardSlots.Count);

        DailyQuest quest = taskCardSlots[randomListNumber].GetComponent<DailyQuest>();

        return quest;

    }

    private PlayerTask GetRandomTask()
    {
        
        List<PlayerTask> tasks = _dtManager.ValidTasks.Tasks;

        int randomListNumber = Random.Range(0, tasks.Count);

        return tasks[randomListNumber];


    }

    public void GenerateTaskOptions()
    {
        for (int i = 0; i < 3; i++)
        {
            CreateTaskCard(GetRandomTask());
        }
    }

    public DailyQuest CreateTaskCard(PlayerTask task)
    {
        var gameVersion = GameConfig.Get().GameVersionType;

        GameObject prefabToInstantiate = (
                _gameVersion == VersionType.Education ?
                _dtManager.GetEducationPrefabCategory(task.EducationCategory) :
                _dtManager.GetNormalPrefabCategory(task.Points)
                );

        GameObject taskCard = Instantiate(prefabToInstantiate, _taskCardHolder);

        DailyQuest quest = taskCard.GetComponent<DailyQuest>();
        quest.SetTaskData(task);
        quest.PopulateData();
        quest.ShowWindowWithType(TaskWindowType.Available);

        taskCard.SetActive(true);

        return quest;
    }
}
