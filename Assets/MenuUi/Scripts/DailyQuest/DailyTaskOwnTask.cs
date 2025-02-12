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
    [Range(0f, 1f)]
    [SerializeField] private float _progressMarkerXScale = 0.05f;
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

    [Header("Player Character")]
    [SerializeField] private Image _playerCharacterImage;

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

        if (amount > _taskProgressMarkers.Count)
            amount = _taskProgressMarkers.Count + 1;

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
