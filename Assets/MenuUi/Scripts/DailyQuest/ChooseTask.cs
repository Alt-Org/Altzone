using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseTask : MonoBehaviour
{

    /// <summary>
    /// This enum is only for testing to be more easy to quickly switch between task selection and random question from the inspector
    /// </summary>
    public enum ChooseTaskPopupType
    {
        None,
        TaskSelection,
        RandomQuestion
    }

    [SerializeField]
    private ChooseTaskPopupType _popupType;

    [Header("Task Selection")]
    [SerializeField]
    [Tooltip("The window where the tasks are shown in")]
    private RectTransform _taskSelectionWindow;

    [SerializeField]
    [Tooltip("The object that's children are going to be the parents of the task cards")]
    private RectTransform _taskCardHolder;

    [Header("Random Question")]
    [SerializeField]
    [Tooltip("The window where the random questions are shown in")]
    private RectTransform _randomQuestionWindow;

    [SerializeField]
    [Tooltip("The background image to scale")]
    private RectTransform _randomQuestionBackground;

    [SerializeField]
    [Tooltip("The question")]
    private TextMeshProUGUI _randomQuestionTitle;

    [SerializeField]
    [Tooltip("The object that's children are going to the Random Question answers")]

    private RectTransform _multipleAnswerHolder;
    [SerializeField]
    [Tooltip("The object that's children are going to the Random Question answers")]
    private RectTransform _doubleAnswerHolder;

    private RectTransform _randomQuestionAnswerHolder;

    [SerializeField]
    [Tooltip("The random question answer prefab")]
    private GameObject _randomQuestionAnswerPrefab;

    [SerializeField]
    [Tooltip("Temporary Holder for tutorial controller until a more parmanent solution is figured out.")]
    private MainMenuTutorialController _tutorial;

    private DailyTaskView _dailyTaskView;
    private VersionType _gameVersion;

    private bool _initialized = false;

    public delegate void ChooseTaskShown();
    public static event ChooseTaskShown OnChooseTaskShown;

    public delegate void ChooseTaskHidden();
    public static event ChooseTaskHidden OnChooseTaskHidden;

    private static bool _shouldShowPopup = false;

    private float _bgStepHeight = 0.06f;
    private float _initialBgSize = 0.55f;

    IEnumerator Initialize()
    {

        Debug.Log("Initializing ChooseTask.cs...");
        if (_initialized)
        {
            Debug.LogWarning("Initializing ChooseTask.cs... Already Initialized?");
            yield break;
        }

        // Wait until player has done their choice on emotion selector
        //yield return new WaitUntil(() => EmotionSelectorPopupScript.EmotionInsertedToday);

        // Wait until DailyTaskManager is ready
        yield return new WaitUntil(() => DailyTaskManager.Instance.DataReady);

        Debug.Log("Initializing ChooseTask.cs... DailyTaskManager ready!");

        // Wait until tutorial has finished
        yield return new WaitUntil(() => !_tutorial.IsTutorialInProgress);
        

        _dailyTaskView = GameObject.FindObjectOfType<DailyTaskView>(true);

        _gameVersion = GameConfig.Get().GameVersionType;


        // Show popup every other battle on turboeducation
        if (_gameVersion == VersionType.TurboEducation && _popupType != ChooseTaskPopupType.None)
        {
            if (DailyTaskProgressManager.Instance.HasOnGoingTask() && DailyTaskManager.Instance.CurrentTaskForced)
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

    /// <summary>
    /// Generate the task options or a random question with answers and show them to the user
    /// </summary>
    public void ShowSelectionWindow()
    {
        if (_popupType == ChooseTaskPopupType.TaskSelection)
        {
            GenerateTaskOptions();
            _taskSelectionWindow.gameObject.SetActive(true);
            OnChooseTaskShown?.Invoke();


        }
        else if (_popupType == ChooseTaskPopupType.RandomQuestion)
        {
            CreateRandomQuestion();
            _randomQuestionWindow.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Gets a random RandomQuestionData from the RandomQuestionConfig and creates the answer options for the question
    /// </summary>
    private void CreateRandomQuestion()
    {
        DeleteRandomQuestionAnswers();

        // Get every question
        List<RandomQuestionData> questions = RandomQuestionConfig.Instance.GetRandomQuestions();

        // Get random question
        System.Random rand = new System.Random();
        int index = rand.Next(questions.Count);

        RandomQuestionData question = questions[index];

        // Show the question title for the player
        _randomQuestionTitle.text = question.Question;

        // Select the proper answer holder
        if (question.answers.Count > 2)
        {
            _randomQuestionAnswerHolder = _multipleAnswerHolder;
            _doubleAnswerHolder.gameObject.SetActive(false);
            _multipleAnswerHolder.gameObject.SetActive(true);

            // Scale background accordingly
            float yPos = _initialBgSize - (_bgStepHeight * question.answers.Count);

            _randomQuestionBackground.anchorMin = new Vector2(
                _randomQuestionBackground.anchorMin.x,
                yPos);
        }
        else
        {
            _randomQuestionAnswerHolder = _doubleAnswerHolder;
            _doubleAnswerHolder.gameObject.SetActive(true);
            _multipleAnswerHolder.gameObject.SetActive(false);

            // Scale background accordingly
            _randomQuestionBackground.anchorMin = new Vector2(
                _randomQuestionBackground.anchorMin.x,
                _initialBgSize);

        }

        // Create answers for the player to select from
        foreach (RandomQuestionAnswer questionAnswer in question.answers)
        {
            GameObject answerObj = Instantiate(_randomQuestionAnswerPrefab, _randomQuestionAnswerHolder);
            answerObj.GetComponentInChildren<TextMeshProUGUI>().text = questionAnswer.Answer;
            answerObj.GetComponentInChildren<Button>().onClick.AddListener(() => { HideSelectionWindow(); });

        }
    }

    /// <summary>
    /// Destroys the current random question answers that are parented by _doubleAnswerHolder and _multipleAnswerHolder
    /// </summary>
    private void DeleteRandomQuestionAnswers()
    {

        for (int i = 0; i < _doubleAnswerHolder.childCount; i++)
        {
            Destroy(_doubleAnswerHolder.GetChild(i).gameObject);
        }

        for (int i = 0; i < _multipleAnswerHolder.childCount; i++)
        {
            Destroy(_multipleAnswerHolder.GetChild(i).gameObject);
        }
    }


    /// <summary>
    /// Delete the task options or the random question answers and hide the selection window
    /// </summary>
    public void HideSelectionWindow(PlayerTask task)
    {
        HideSelectionWindow();
    }

    public void HideSelectionWindow()
    {
        if (_taskSelectionWindow.gameObject.activeSelf)
        {
            _taskSelectionWindow.gameObject.SetActive(false);
            DeleteTaskCards();
            DailyTaskManager.Instance.CurrentTaskForced = true;
            OnChooseTaskHidden?.Invoke();


        }
        if (_randomQuestionWindow.gameObject.activeSelf)
        {
            _randomQuestionWindow.gameObject.SetActive(false);
            DeleteRandomQuestionAnswers();
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

        int i = 0;
        // Create three task cards for the categories
        foreach (EducationCategoryType category in ed)
        {
            // Create the task card's under _taskCardHolder's children (taskcardSlots)
            _dailyTaskView.CreateTaskCard(GetRandomTaskFromCategory(category), _taskCardHolder.GetChild(i));
            i++;
        }
    }

    /// <summary>
    /// Destroys the current task cards that are parented by _taskCardHolder's children (taskcardSlots)
    /// </summary>
    private void DeleteTaskCards()
    { 
        for (int i = 0; i < _taskCardHolder.childCount; i++)
        {

            if (_taskCardHolder.GetChild(i).childCount > 0)
            {
                Destroy(_taskCardHolder.GetChild(i).GetChild(0).gameObject);
            }
            
        }
    }
}
