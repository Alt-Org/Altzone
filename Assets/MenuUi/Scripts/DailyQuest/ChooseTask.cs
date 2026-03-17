using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using static DailyQuest;

public class ChooseTask : MonoBehaviour
{

    [SerializeField]
    [Tooltip("The window where the tasks are shown in")]
    private RectTransform _selectionWindow;

    [SerializeField]
    [Tooltip("The parent for the task cards")]
    private RectTransform _taskCardHolder;

    private DailyTaskManager _dtManager;
    private VersionType _gameVersion;

    private void Start()
    {
        _dtManager = GameObject.Find("DailyTaskManager").GetComponent<DailyTaskManager>();
        _gameVersion = GameConfig.Get().GameVersionType;

        DailyTaskProgressManager.OnTaskChange += HideSelectionWindow;
        if (_gameVersion == VersionType.TurboEducation)
        {
            //ShowSelectionWindow();
        }
    }



    /// <summary>
    /// Generate the task options and show them to the user
    /// </summary>
    public void ShowSelectionWindow()
    {
        GenerateTaskOptions();
        _selectionWindow.gameObject.SetActive(true);
    }

    /// <summary>
    /// Delete the task options and hide the selection window
    /// </summary>
    public void HideSelectionWindow(PlayerTask task)
    {
        _selectionWindow.gameObject.SetActive(false);
        DeleteTaskCards();
    }


    /// <summary>
    /// Get three random EducationCategoryTypes
    /// </summary>
    /// <returns>Three different EducationCategoryTypes</returns>
    private EducationCategoryType[] GetRandomCategories()
    {

        // Get all of the education categorytypes
        Array enumsArray = Enum.GetValues(typeof(EducationCategoryType));

        List<EducationCategoryType> educationCategories = new List<EducationCategoryType>((IEnumerable<EducationCategoryType>)enumsArray);
        educationCategories.Remove(EducationCategoryType.None); // Remove none, because there are no tasks for this one

        // A list that will be returned
        EducationCategoryType[] finalCategories = new EducationCategoryType[3];

        // Loop three times to get three different categories
        for (int i = 0; i < 3; i++)
        {
            // Get a random category
            int randomNum = UnityEngine.Random.Range(0, educationCategories.Count);
            EducationCategoryType randomCat = educationCategories[randomNum];

            // Max tries to avoid infinite loop
            int maxTries = 5;

            // If the selected random category exists, choose a new one
            while (maxTries > 0 && finalCategories.Contains(randomCat))
            {
                randomNum = UnityEngine.Random.Range(0, educationCategories.Count);
                randomCat = educationCategories[randomNum];
                maxTries--;
            }

            // Add the selected random category to the list
            finalCategories[i] = randomCat;
        }

        return finalCategories;

    }

    /// <summary>
    /// Gets a random task from the given EducationCategoryType
    /// </summary>
    /// <param name="category">The EducationCategoryType to look for a task for</param>
    /// <returns>A random task from the given category</returns>
    private PlayerTask GetRandomTaskFromCategory(EducationCategoryType category)
    {
        // Get all tasks from DailyTaskManager
        List<PlayerTask> tasks = _dtManager.ValidTasks.Tasks;

        List<PlayerTask> categoryTasks = new List<PlayerTask>();

        // Sort out only the tasks that belong in to the given category
        foreach (PlayerTask task in tasks)
        {
            if (task.EducationCategory == category) categoryTasks.Add(task);
        }

        // If somehow there are no tasks for this category, return null
        if (categoryTasks.Count == 0) return null;

        // Get a random task from the category and return it
        int randomListNumber = UnityEngine.Random.Range(0, categoryTasks.Count);


        return categoryTasks[randomListNumber];


    }

    /// <summary>
    /// Generate the Task Options,create the task cards and set them in place
    /// </summary>
    public void GenerateTaskOptions()
    {
        DeleteTaskCards();

        // Get three random categories
        EducationCategoryType[] ed = GetRandomCategories();

        // Create three task cards for the categories
        foreach (EducationCategoryType category in ed)
        {
            _dtManager.CreateTaskCard(GetRandomTaskFromCategory(category), _taskCardHolder);
        }
    }

    /// <summary>
    /// Destroys the current task cards (parented by _taskCardHolder)
    /// </summary>
    private void DeleteTaskCards()
    {
        for (int i = 0; i < _taskCardHolder.childCount; i++)
        {
            
            Destroy(_taskCardHolder.GetChild(i).gameObject);
        }
    }
}
