using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.TabLine;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskView : AltMonoBehaviour
{

    [SerializeField] private TabLine _tabline;

    [Header("Views")]
    [SerializeField] private GameObject _dailyTasksView;
    [SerializeField] private GameObject _ownTaskView;
    [SerializeField] private GameObject _clanTaskView;

    [Header("TabButtons")]
    [SerializeField] private Button _dailyTasksTabButton;
    [SerializeField] private Button _ownTaskTabButton;
    [SerializeField] private Button _clanTaskTabButton;

    [Header("DailyTasksEducationPage")]
    [SerializeField] private GameObject _dailyTasksEducationView;
    [SerializeField] private RectTransform _tasksEducationVerticalLayout;
    [Space]
    [SerializeField] private Transform _taskListEducationSocial;
    [SerializeField] private Transform _taskListEducationStory;
    [SerializeField] private Transform _taskListEducationCulture;
    [SerializeField] private Transform _taskListEducationEthical;
    [SerializeField] private Transform _taskListEducationAction;

    [Header("DailyTasksNormalPage")]
    [Space]
    [SerializeField] private GameObject _dailyTasksNormalView;
    [SerializeField] private RectTransform _tasksNormalVerticalLayout;
    [Space]
    [SerializeField] private Transform _taskListNormalRow1;
    [SerializeField] private int _dailyCategoryNormalRow1PointsLimit = 100;
    [Space]
    [SerializeField] private Transform _taskListNormalRow2;
    [SerializeField] private int _dailyCategoryNormalRow2PointsLimit = 500;
    [Space]
    [SerializeField] private Transform _taskListNormalRow3;

    private List<GameObject> _dailyTaskCardSlots = new List<GameObject>();
    [HideInInspector] public List<GameObject> DailyTaskCardSlots { get { return _dailyTaskCardSlots; } }

    private VersionType _gameVersion;

    public enum SelectedTab
    {
        Tasks,
        OwnTask,
        ClanTask
    }
    private SelectedTab _selectedTab = SelectedTab.Tasks;

    private DailyTaskManager _dtManager;

    private void Start()
    {
        _dtManager = GameObject.Find("DailyTaskManager").GetComponent<DailyTaskManager>();

        ViewSetup();

        //Buttons
        _dailyTasksTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.Tasks));
        _ownTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.OwnTask));
        _clanTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.ClanTask));
    }


    /// <summary>
    /// Switch tabs on the quest page
    /// </summary>
    /// <param name="tab">Tab to switch to</param>
    public void SwitchTab(SelectedTab tab)
    {
        //Hide old tab
        switch (_selectedTab)
        {
            case SelectedTab.Tasks: _dailyTasksView.SetActive(false); break;
            case SelectedTab.OwnTask: _ownTaskView.SetActive(false); break;
            default: _clanTaskView.SetActive(false); break;
        }

        // Set new selected tab
        _selectedTab = tab;

        //Show new tab
        switch (tab)
        {
            case SelectedTab.Tasks: _dailyTasksView.SetActive(true); break;
            case SelectedTab.OwnTask: _ownTaskView.SetActive(true); break;
            default: _clanTaskView.SetActive(true); break;
        }
        _tabline.ActivateTabButton((int)_selectedTab);

        Debug.Log($"Switched to {_selectedTab}.");
    }


    /// <summary>
    /// DailyTask page setup
    /// </summary>
    private void ViewSetup()
    {
        _gameVersion = GameConfig.Get().GameVersionType;

        
        if (_gameVersion is VersionType.Education or VersionType.TurboEducation)
        {
            _dailyTasksEducationView.gameObject.SetActive(true);
            _dailyTasksNormalView.gameObject.SetActive(false);
        }
        else
        {
            _dailyTasksEducationView.gameObject.SetActive(false);
            _dailyTasksNormalView.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Creates a task card for the given task and adds it to the view
    /// </summary>
    /// <param name="task">The task to create a task card for</param>
    /// <returns>DailyQuest from the given PlayerTask</returns>
    public DailyQuest AddTaskCardToView(PlayerTask task)
    {

        GameObject prefabToInstantiate = (
                _gameVersion is VersionType.Education or VersionType.TurboEducation ?
                _dtManager.GetEducationPrefabCategory(task.EducationCategory) :
                _dtManager.GetNormalPrefabCategory(task.Points)
                );

        Transform parentCategory = (
            _gameVersion is VersionType.Education or VersionType.TurboEducation ?
            GetEducationParentCategory(task.EducationCategory) :
            GetNormalParentCategory(task.Points)
            );

        DailyQuest quest = _dtManager.CreateTaskCard(task, parentCategory);

        return quest;
    }


    public Transform GetNormalParentCategory(int points)
    {
        if (points < _dailyCategoryNormalRow1PointsLimit)
            return (_taskListNormalRow1);

        if (points < _dailyCategoryNormalRow2PointsLimit)
            return (_taskListNormalRow2);

        return (_taskListNormalRow3);
    }
    public Transform GetEducationParentCategory(EducationCategoryType type)
    {
        return type switch
        {
            <= EducationCategoryType.Social => _taskListEducationSocial,
            <= EducationCategoryType.Story => _taskListEducationStory,
            <= EducationCategoryType.Culture => _taskListEducationCulture,
            <= EducationCategoryType.Ethical => _taskListEducationEthical,
            <= EducationCategoryType.Action => _taskListEducationAction,
            _ => _taskListEducationSocial,
        };
    }

    public void UpdateDailyTaskCards()
    {
        if (_gameVersion is VersionType.Education or VersionType.TurboEducation)
        {
            //Needed to update the instantiated DT cards spacing in HorizontalLayoutGroups.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationSocial.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationStory.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationCulture.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationEthical.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationAction.GetComponent<RectTransform>());

            //Sets DT cards to left side.
            _taskListEducationSocial.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListEducationStory.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListEducationCulture.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListEducationEthical.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListEducationAction.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            //Sets DT card category list to the top.
            _tasksEducationVerticalLayout.anchoredPosition = Vector2.zero;
        }
        else
        {
            //Needed to update the instantiated DT cards spacing in HorizontalLayoutGroups.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListNormalRow1.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListNormalRow2.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListNormalRow3.GetComponent<RectTransform>());

            //Sets DT cards to left side.
            _taskListNormalRow1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListNormalRow2.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListNormalRow3.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            //Sets DT card category list to the top.
            _tasksNormalVerticalLayout.anchoredPosition = Vector2.zero;
        }
    }

}
