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
    [SerializeField] private GameObject _taksProgressMarkerPrefab;

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

    public IEnumerator SetDailyTask(string taskDescription, int amount, int points, int coins)
    {
        _taskDescription.text = taskDescription;
        _taskPointsReward.text = "" + points;
        _taskCoinsReward.text = "" + coins;

        SetProgressBar(amount);

        yield return (true);
    }

    private void SetProgressBar(int amount)
    {
        if (amount > _taskProgressMarkers.Count)
            CreateProgressBarMarkers(amount - _taskProgressMarkers.Count);

        DeactivateAllProgressBarMarkers();

        for (int i = 0; i < (amount - 1); i++)
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
            GameObject marker = Instantiate(_taksProgressMarkerPrefab, _taskProgressLayoutGroup);
            _taskProgressMarkers.Add(marker);
        }
    }

    public IEnumerator SetProgress(float progress)
    {
        _taskProgressFillImage.fillAmount = progress;

        yield return new WaitUntil(() => true);
    }

    public IEnumerator SetStipend(int points, int coins, int rank)
    {
        _stipendTotalPointsForClan.text = "" + points;
        _stipendTotalCoinsForClan.text = "" + coins;
        _stipendClanRankValue.text = "" + rank;

        return (null);
        //yield return new WaitUntil(() => );
    }

    public IEnumerator SetClockTime(int time)
    {


        return (null);
        //yield return new WaitUntil(() => );
    }
}
