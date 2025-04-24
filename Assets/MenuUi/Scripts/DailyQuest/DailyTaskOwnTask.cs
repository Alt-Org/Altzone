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
    [SerializeField] private TextMeshProUGUI _taskPointsReward;
    [SerializeField] private TextMeshProUGUI _taskCoinsReward;
    [SerializeField] private Image _taskTypeImage;
    [Space]
    [SerializeField] private Image _taskProgressFillImage;
    [SerializeField] private RectTransform _taskProgressLayoutGroup;
    [SerializeField] private GameObject _taskProgressMarkerPrefab;
    [SerializeField] private int _progressMarkersMaxAmount = 8;
    [Range(0f, 1f)]
    [SerializeField] private float _progressMarkerXScale = 0.05f;
    [SerializeField] private TMP_Text _testTaskProgressValue; //TODO: Remove when testing done.

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

    private void Start()
    {
        CreateProgressBarMarkers(_progressMarkersMaxAmount);
        SetMood(MoodType.Ok);
    }

    #region Task

    public IEnumerator SetDailyTask(PlayerTask data)
    {
        _taskDescription.text = data.Title;
        _taskPointsReward.text = "" + data.Points;
        _taskCoinsReward.text = "" + data.Coins;
        _taskTypeImage.sprite = _cardImageReference.GetTaskImage(data);

        yield return new WaitUntil(() => (_taskProgressMarkers.Count != 0));

        SetProgressBarMarkers(data.Amount);
    }

    /// <summary>
    /// Set the amount of visible progress bar markers.
    /// </summary>
    private void SetProgressBarMarkers(int amount)
    {
        DeactivateAllProgressBarMarkers();

        if (amount > _taskProgressMarkers.Count)
            amount = _taskProgressMarkers.Count + 1;

        //Activate needed amount or all progress bar markers and set their locations.
        for (int i = 0; (i < (amount - 1) && i < _taskProgressMarkers.Count); i++)
        {
            _taskProgressMarkers[i].SetActive(true);
            _taskProgressMarkers[i].GetComponent<RectTransform>().anchorMin = new(((float)(i + 1) / (float)amount), 0f);
            _taskProgressMarkers[i].GetComponent<RectTransform>().anchorMax = new(((float)(i + 1) / (float)amount) + _progressMarkerXScale, 1f);
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
            _taskProgressMarkers.Add(marker);
        }
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
        _taskDescription.text = "";
        _taskPointsReward.text = "";
        _taskCoinsReward.text = "";

        SetProgressBarMarkers(0);
    }

    #endregion

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
