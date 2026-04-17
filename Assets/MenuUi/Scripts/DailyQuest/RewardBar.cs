using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;
using UnityEngine.UI;

public class RewardBar : MonoBehaviour
{

    [SerializeField] private Slider _rewardBarSlider;
    [SerializeField] private RectTransform _rewardBarMarkersBase;
    [SerializeField] private GameObject _rewardBarMarkerPrefab;

    private List<GameObject> _rewardBarMarkers;
    private int _rewardBarGoal = 2000;

    ClanData _clanData;


    //private int _clanMilestoneLatestRewardIndex = -1;
    //private int _currentPoints = 0;


    public void Initialize(ClanData clanData)
    {
        _clanData = clanData;
        CreateBar();
    }

    private void OnEnable()
    {
        if (_clanData != null)
        {
            UpdateBar();
        }
    }

    

    private void CreateBar()
    {

        _rewardBarMarkers = new List<GameObject>();

        // TODO: Calculate correct position for markers on bar
        foreach (DailyTaskClanReward.ClanRewardData data in GetBarRewards())
        {
            GameObject rewardMarker = Instantiate(_rewardBarMarkerPrefab, _rewardBarMarkersBase);
            rewardMarker.GetComponent<DailyTaskClanReward>().Set(data);
            _rewardBarMarkers.Add(rewardMarker);
        }

        UpdateBar();
    }

    private void UpdateBar()
    {
        _rewardBarSlider.value = (float)_clanData.Points / (float)_rewardBarGoal;
        Debug.Log("Slider: " + _rewardBarSlider.value + " Value: " + ((float)_clanData.Points / (float)_rewardBarGoal));
    }


    /// <summary>
    /// Gets the bar rewards from server
    /// </summary>
    /// <returns>The bar rewards</returns>
    [Obsolete("Now they are placeholder rewards, later on they should be fetched from the server")]
    private List<DailyTaskClanReward.ClanRewardData> GetBarRewards()
    {
        // Now they are placeholder rewards, later on they should be fetched from the server

        var clanRewardDatas = new List<DailyTaskClanReward.ClanRewardData>()
        {
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Box, 300, null, UnityEngine.Random.Range(1,250)),
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Box, 600, null, UnityEngine.Random.Range(250,500)),
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Box, 1200, null, UnityEngine.Random.Range(500,750)),
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Chest, 2000, null, 1000)
        };

        return clanRewardDatas;
    }

    public void AddPoints(int value)
    {
        _clanData.Points += value; // TODO: This should be done on server
        UpdateBar();
        CheckBarProgress();
        //StartCoroutine(CalculateClanRewardBarProgress());
    }

    /// <summary>
    /// Checks the progress and updates if rewards/markers are reached
    /// </summary>
    private void CheckBarProgress()
    {

        foreach (GameObject marker in _rewardBarMarkers)
        {
            DailyTaskClanReward reward = marker.GetComponent<DailyTaskClanReward>();

            if (_clanData.Points >= reward.Data.Threshold)
            {
                
                if (!reward.Data.Open)
                {
                    Debug.Log("Points: " + _clanData.Points + " Thresh: " + reward.Data.Threshold);
                    ReachReward(reward);
                }
            }
        }
    }

    /// <summary>
    /// Updates the reward state to reached
    /// </summary>
    /// <param name="reward">The reward that should be reached</param>
    private void ReachReward(DailyTaskClanReward reward)
    {
        reward.UpdateState(true);
        DailyTaskProgressManager.Instance.InvokeOnClanMilestoneReached();
    }


    /*private IEnumerator CalculateClanRewardBarProgress()
    {
        float sectionLengths = (1f / (float)_rewardBarMarkers.Count);

        // Loop through every marker
        for (int i = 0; i < _rewardBarMarkers.Count; i++)
        {
            int startPoints = (
                (i) <= 0 ?
                0 :
                _rewardBarMarkers[i - 1].GetComponent<DailyTaskClanReward>().Data.Threshold
                );

            int endPoints = _rewardBarMarkers[i].GetComponent<DailyTaskClanReward>().Data.Threshold;

            if ((_currentPoints < endPoints) || (i >= _rewardBarMarkers.Count - 1))
            {
                float startPosition = sectionLengths * i;
                float endPosition = ((i + 1) >= _rewardBarMarkers.Count ? 1f : (sectionLengths * (float)(i + 1)));

                float chunkProgress = (float)(_currentPoints - startPoints) / (float)(endPoints - startPoints);
                Debug.Log("ClanRewardsProgressBar: chunk progress: " + chunkProgress + ", start points: " + startPoints + ", end points: " + endPoints);

                //All but final reward.
                for (int j = 0; j < i; j++)
                {
                    if (j <= _clanMilestoneLatestRewardIndex)
                        continue;

                    _clanMilestoneLatestRewardIndex = j;
                    _rewardBarMarkers[j].GetComponent<DailyTaskClanReward>().UpdateState(true);
                    DailyTaskProgressManager.Instance.InvokeOnClanMilestoneReached(); //TODO: Remove when server ready.
                }

                //Final reward
                if ((i >= _rewardBarMarkers.Count - 1) && chunkProgress == 1)
                {
                    _rewardBarMarkers[_rewardBarMarkers.Count - 1].GetComponent<DailyTaskClanReward>().UpdateState(true);
                    DailyTaskProgressManager.Instance.InvokeOnClanMilestoneReached(); //TODO: Remove when server ready.
                }

                _rewardBarSlider.value = Mathf.Lerp(startPosition, endPosition, chunkProgress);
                break;
            }
        }

        yield return true;
    }*/

}
