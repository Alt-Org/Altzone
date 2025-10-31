using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskOwnTask : MonoBehaviour
{
    public enum MoodType
    {
        NoWork,
        Lazy,
        Ok,
        Headache,
        Depressed
    }

    [SerializeField] private DailyTaskCardImageReference _cardImageReference;

    [Header("Current task")]
    [SerializeField] private TextMeshProUGUI _taskDescription;
    [SerializeField] private TextMeshProUGUI _taskCategory;
    [SerializeField] private GameObject _taskRewardsField;
    [SerializeField] private TextMeshProUGUI _taskPointsReward;
    [SerializeField] private TextMeshProUGUI _taskCoinsReward;
    [SerializeField] private Image _taskTypeImage;
    [SerializeField] private Image _taskBackground;
    [Space]
    [SerializeField] private Image _taskProgressFillImage;
    [SerializeField] private RectTransform _taskProgressLayoutGroup;
    [SerializeField] private GameObject _taskProgressMarkerPrefab;
    [SerializeField] private int _progressMarkersMaxAmount = 8;
    [Range(0f, 2f)]
    [SerializeField] private float _progressMarkerXScale = 0.05f;
    [SerializeField] private TMP_Text _testTaskProgressValue; //TODO: Remove when testing done.

    private PlayerTask _currentTask;

    private List<GameObject> _taskProgressMarkers = new List<GameObject>();

    [Header("Clock")]
    [SerializeField] private Image _taskTimeLeftImage;
    [SerializeField] private TextMeshProUGUI _taskTimeLeftValue;

    [Header("Stipend")]
    [SerializeField] private TextMeshProUGUI _stipendTotalPointsForClan;
    [SerializeField] private TextMeshProUGUI _stipendTotalCoinsForClan;
    [SerializeField] private TextMeshProUGUI _stipendClanRankValue;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _randomText;

    [Header("Player Character")]
    [SerializeField] private Image _playerCharacterImage;
    [Space]
    [SerializeField] private GameObject _breakReminder;
    [Space]
    [SerializeField] private GameObject _workMoodNoWork;
    [SerializeField] private GameObject _workMoodLazy;
    [SerializeField] private GameObject _workMoodOk;
    [SerializeField] private GameObject _workMoodHeadache;
    [SerializeField] private GameObject _workMoodDepressed;

    private MoodType _moodType = MoodType.Ok;

    [Header("Education Category Colors")]
    [SerializeField] private Color _actionCategoryColor;
    [SerializeField] private Color _socialCategoryColor;
    [SerializeField] private Color _storyCategoryColor;
    [SerializeField] private Color _cultureCategoryColor;
    [SerializeField] private Color _ethicalCategoryColor;
    [SerializeField] private Color _defaultColor;

    private void Start()
    {
        CreateProgressBarMarkers(_progressMarkersMaxAmount);
        SetMood(MoodType.Ok);
        SettingsCarrier.OnLanguageChanged += UpdateLanguage;
    }

    private void OnDestroy()
    {
        SettingsCarrier.OnLanguageChanged -= UpdateLanguage;
    }

    #region Task

    public IEnumerator SetDailyTask(PlayerTask data)
    {
        _currentTask = data;
        SetTaskTitle(data, SettingsCarrier.Instance.Language);
        SetTaskCategory(data, SettingsCarrier.Instance.Language);
        _taskPointsReward.text = "" + data.Points;
        _taskCoinsReward.text = "" + data.Coins;
        _taskRewardsField.SetActive(true);
        _taskTypeImage.sprite = _cardImageReference.GetTaskImage(data);
        _taskTypeImage.enabled = true;

        yield return new WaitUntil(() => (_taskProgressMarkers.Count != 0));

        SetProgressBarMarkers(data.Amount);
    }

    /// <summary>
    /// Set the amount of visible progress bar markers.
    /// </summary>
    private void SetProgressBarMarkers(int amount)
    {
        if (amount > _taskProgressMarkers.Count)
            amount = _taskProgressMarkers.Count + 1;

        //Activate needed amount or all progress bar markers and set their locations.
        for (int i = 0; (i < (amount - 1) && i < _taskProgressMarkers.Count); i++)
        {
            _taskProgressMarkers[i].SetActive(true);
            _taskProgressMarkers[i].GetComponent<RectTransform>().anchorMin = new(((float)(i + 1) / (float)amount), 0f);
            _taskProgressMarkers[i].GetComponent<RectTransform>().anchorMax = new(((float)(i + 1) / (float)amount), 1f);
        }
    }

    private void DeactivateAllProgressBarMarkers()
    {
        foreach (var marker in _taskProgressMarkers)
        {
            marker.gameObject.SetActive(false);
        }
    }

    private void CreateProgressBarMarkers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject marker = Instantiate(_taskProgressMarkerPrefab, _taskProgressLayoutGroup);
            marker.GetComponent<RectTransform>().sizeDelta = new Vector2(_progressMarkerXScale, 1f);
            _taskProgressMarkers.Add(marker);
        }

        DeactivateAllProgressBarMarkers();
    }

    /// <summary>
    /// Set the current tasks visual progress bar fill amount.
    /// </summary>
    public void SetTaskProgress(float progress)
    {
        _taskProgressFillImage.fillAmount = progress;
    }

    public void TESTSetTaskValue(int progress) //TODO: Remove when testing done.
    {
        _testTaskProgressValue.text = "" + progress;
    }

    public void ClearCurrentTask()
    {
        _currentTask = null;
        _taskDescription.text = "";
        _taskCategory.text = "";
        _taskPointsReward.text = "";
        _taskCoinsReward.text = "";
        _taskRewardsField.SetActive(false);
        _taskTypeImage.enabled = false;
        _taskBackground.color = _defaultColor;

        SetProgressBarMarkers(0);
    }

    private void SetTaskTitle(PlayerTask task, SettingsCarrier.LanguageType language)
    {
        _taskDescription.text = language == SettingsCarrier.LanguageType.Finnish ? task.Title : task.EnglishTitle;
    }

    private void SetTaskCategory(PlayerTask task, SettingsCarrier.LanguageType language)
    {
        if (task.Type != TaskNormalType.Undefined)
        {
            _taskCategory.text = "";
        }
        else
        {
            switch (task.EducationCategory)
            {
                case EducationCategoryType.Action:
                    {
                        _taskCategory.text = language == SettingsCarrier.LanguageType.Finnish ? "Toiminnallinen pelilukutaito" : "Functional game literacy";
                        _taskBackground.color = _actionCategoryColor;
                        break;
                    }
                case EducationCategoryType.Social:
                    {
                        _taskCategory.text = language == SettingsCarrier.LanguageType.Finnish ? "Sosiaalinen pelilukutaito" : "Social game literacy";
                        _taskBackground.color = _socialCategoryColor;
                        break;
                    }
                case EducationCategoryType.Story:
                    {
                        _taskCategory.text = language == SettingsCarrier.LanguageType.Finnish ? "Tarinallinen pelilukutaito" : "Story-based game literacy";
                        _taskBackground.color = _storyCategoryColor;
                        break;
                    }
                case EducationCategoryType.Culture:
                    {
                        _taskCategory.text = language == SettingsCarrier.LanguageType.Finnish ? "Kulttuurinen pelilukutaito" : "Cultural game literacy";
                        _taskBackground.color = _cultureCategoryColor;
                        break;
                    }
                case EducationCategoryType.Ethical:
                    {
                        _taskCategory.text = language == SettingsCarrier.LanguageType.Finnish ? "Eettinen pelilukutaito" : "Ethical game literacy";
                        _taskBackground.color = _ethicalCategoryColor;
                        break;
                    }
                default:
                    {
                        _taskCategory.text = "";
                        _taskBackground.color = _defaultColor;
                        break;
                    }
            }
        }
    }
    #endregion

    private void UpdateLanguage(SettingsCarrier.LanguageType language)
    {
        if (_currentTask != null)
        {
            SetTaskTitle(_currentTask, language);
            SetTaskCategory(_currentTask, language);
        }
    }

    public void SetMood(MoodType type)
    {
        //Turn off old mood.
        switch (_moodType)
        {
            case MoodType.NoWork: break;
            case MoodType.Lazy: break;
            case MoodType.Ok: break;
            case MoodType.Headache: _workMoodHeadache.SetActive(false); break;
            case MoodType.Depressed: _workMoodDepressed.SetActive(false); break;
        }

        _moodType = type;

        //Turn on new mood.
        switch (type)
        {
            case MoodType.NoWork: break;
            case MoodType.Lazy: break;
            case MoodType.Ok: break;
            case MoodType.Headache: _workMoodHeadache.SetActive(true); break;
            case MoodType.Depressed: _workMoodDepressed.SetActive(true); break;
        }

        //Take a break reminder.
        if (_moodType == MoodType.Headache || _moodType == MoodType.Depressed)
            _breakReminder.SetActive(true);
        else
            _breakReminder.SetActive(false);
    }

    public void SetStipend(int points, int coins, int rank)
    {
        _stipendTotalPointsForClan.text = "" + points;
        _stipendTotalCoinsForClan.text = "" + coins;
        _stipendClanRankValue.text = "" + rank;
    }

    public void SetClockTime(int time)
    {


    }
}
