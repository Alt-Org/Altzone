using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;

public class ChooseTask : MonoBehaviour
{

    [SerializeField]
    [Tooltip("The window where the tasks are shown in")]
    private RectTransform _selectionWindow;

    [SerializeField]
    [Tooltip("The parent for the task cards")]
    private RectTransform _taskCardHolder;

    [SerializeField]
    [Tooltip("The UI overlay to disable the buttons in when the ChooseTask window is active")]
    private RectTransform _UIOverlay;

    [SerializeField]
    [Tooltip("Temporary Holder for tutorial controller until a more parmanent solution is figured out.")]
    private MainMenuTutorialController _tutorial;

    private List<Button> _UIOverlayButtons;

    private DailyTaskView _dailyTaskView;
    private VersionType _gameVersion;

    //[SerializeField]private NaviButton _dailyTaskNaviButton;

    private bool _initialized = false;

    public delegate void ChooseTaskShown();
    public static event ChooseTaskShown OnChooseTaskShown;

    public delegate void ChooseTaskHidden();
    public static event ChooseTaskHidden OnChooseTaskHidden;

    private static bool _shouldShowPopup = false;

    IEnumerator Initialize()
    {

        Debug.Log("Initializing ChooseTask.cs...");
        if (_initialized)
        {
            Debug.LogWarning("Initializing ChooseTask.cs... Already Initialized?");
            yield break;
        }

        // Wait until player has done their choice on emotion selector
        yield return new WaitUntil(() => EmotionSelectorPopupScript.EmotionInsertedToday);

        // Wait until DailyTaskManager is ready
        yield return new WaitUntil(() => DailyTaskManager.Instance.DataReady);

        // Wait until tutorial has finished
        yield return new WaitUntil(() => !_tutorial.IsTutorialInProgress);

        Debug.Log("Initializing ChooseTask.cs... DailyTaskManager ready!");

        _dailyTaskView = GameObject.FindObjectOfType<DailyTaskView>(true);

        _gameVersion = GameConfig.Get().GameVersionType;


        // Show popup every other battle on turboeducation
        if (_gameVersion == VersionType.TurboEducation)
        {
            if (DailyTaskProgressManager.Instance.HasOnGoingTask())
            {
                _shouldShowPopup = false;
            }
            else
            {
                if (_shouldShowPopup)
                {
                    ShowSelectionWindow();
                }

                _shouldShowPopup = !_shouldShowPopup;
            }
        }

        _initialized = true;
        Debug.Log("Initializing ChooseTask.cs... Initialized!");
    }

    public void InitializeChooseTask()
    {
        StartCoroutine(Initialize());
    }

    private void Start()
    {
        DailyTaskProgressManager.OnTaskChange += HideSelectionWindow;
    }

    private void OnDestroy()
    {
        DailyTaskProgressManager.OnTaskChange -= HideSelectionWindow;
    }

    private void OnApplicationQuit()
    {
        _shouldShowPopup = false;
    }

    IEnumerator SwitchViewAndShowWindow()
    {
        yield return new WaitForSeconds(0.2f); //Placeholder
        // Wait for view to switch before showing the window
        //yield return _dailyTaskNaviButton.StartCoroutine(_dailyTaskNaviButton.Navigate()); // Switch to dailytask view
        Debug.Log("Initializing ChooseTask.cs... DailyTaskView ready!");

        ShowSelectionWindow();
    }

    /// <summary>
    /// Generate the task options and show them to the user
    /// </summary>
    public void ShowSelectionWindow()
    {
        GenerateTaskOptions();
        _selectionWindow.gameObject.SetActive(true);
        OnChooseTaskShown?.Invoke();
        //EnableUIOverlayButtons(false);
    }


    /// <summary>
    /// Delete the task options and hide the selection window
    /// </summary>
    public void HideSelectionWindow(PlayerTask task)
    {
        HideSelectionWindow();
    }

    public void HideSelectionWindow()
    {
        _selectionWindow.gameObject.SetActive(false);
        DeleteTaskCards();
        OnChooseTaskHidden?.Invoke();
        //EnableUIOverlayButtons(true);
    }

    /// <summary>
    /// Enables / Disables the buttons on UI overlay
    /// </summary>
    /// <param name="enable">If the buttons should be enabled or not</param>
    private void EnableUIOverlayButtons(bool enable)
    {
        // Store currently enabled buttons to UIOverlayButtons to avoid enabling buttons that should not be enabled
        if (!enable)
        {
            _UIOverlayButtons = new List<Button>();

            // Get every button under UIOverlay
            foreach (Button btn in _UIOverlay.gameObject.GetComponentsInChildren<Button>())
            {
                // If button is interactable (enabled), add it to the list 
                if (btn.interactable) _UIOverlayButtons.Add(btn);
            }
        }

        if (_UIOverlayButtons == null) return; // Should never happen, just a backup

        // Loop through buttons in list and set them to enabled / disabled
        foreach (Button btn in _UIOverlayButtons)
        {
            btn.interactable = enable;
        }

        
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
        List<PlayerTask> tasks = DailyTaskManager.Instance.ValidTasks.Tasks;

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
            _dailyTaskView.CreateTaskCard(GetRandomTaskFromCategory(category), _taskCardHolder);
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
