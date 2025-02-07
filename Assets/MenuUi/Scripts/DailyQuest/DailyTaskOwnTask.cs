using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskOwnTask : MonoBehaviour
{
    [Header("Current task")]
    [SerializeField] private TextMeshProUGUI _taskDescription;
    [SerializeField] private TextMeshProUGUI _taskPointsReward;
    [SerializeField] private TextMeshProUGUI _taskCoinsReward;
    [Space]
    [SerializeField] private Image _taskProgressFillImage;
    [SerializeField] private RectTransform _taskProgressLayoutGroup;
    [SerializeField] private GameObject _taskProgressMarkerPrefab;
    [SerializeField] private int _progressMarkersMaxAmount = 8;
    [SerializeField] private TMP_Text _testTaskProgressValue;

    private List<GameObject> _taskProgressMarkers = new List<GameObject>();

    [Header("Clock")]
    [SerializeField] private Image _taskTimeLeftImage;
    [SerializeField] private TextMeshProUGUI _taskTimeLeftValue;

    [Header("Stipend")]
    [SerializeField] private TextMeshProUGUI _stipendTotalPointsForClan;
    [SerializeField] private TextMeshProUGUI _stipendTotalCoinsForClan;
    [SerializeField] private TextMeshProUGUI _stipendClanRankValue;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _breakRemainder;
    [SerializeField] private TextMeshProUGUI _randomText;

    private void Start()
    {
        CreateProgressBarMarkers(_progressMarkersMaxAmount);
    }

    #region SetTask
    public void SetDailyTask(string taskDescription, int amount, int points, int coins)
    {
        _taskDescription.text = taskDescription;
        _taskPointsReward.text = "" + points;
        _taskCoinsReward.text = "" + coins;

        SetProgressBar(amount);
    }

    private void SetProgressBar(int amount)
    {
        DeactivateAllProgressBarMarkers();

        for (int i = 0; (i < (amount - 1) && i < _taskProgressMarkers.Count); i++)
        {
            _taskProgressMarkers[i].SetActive(true);
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
    #endregion

    public void ClearCurrentTask()
    {
        _taskDescription.text = "";
        _taskPointsReward.text = "";
        _taskCoinsReward.text = "";

        SetProgressBar(0);
    }

    public void SetTaskProgress(float progress)
    {
        _taskProgressFillImage.fillAmount = progress;
    }

    public void TESTSetTaskValue(int progress)
    {
        _testTaskProgressValue.text = "" + progress;
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
