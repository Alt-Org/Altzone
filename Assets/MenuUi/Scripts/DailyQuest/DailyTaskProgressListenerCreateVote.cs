using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;

public class DailyTaskProgressListenerCreateVote : DailyTaskProgressListener
{
    private void Awake()
    {
        _educationCategoryType = EducationCategoryType.Social;
        _educationCategorySocialType = TaskEducationSocialType.CreateNewVote;
    }

    private void OnEnable()
    {
        PollManager.OnPollCreated += OnPollCreated;
    }

    private void OnDisable()
    {
        PollManager.OnPollCreated -= OnPollCreated;
    }

    private void OnPollCreated()
    {
        UpdateProgress("1");
    }
}
