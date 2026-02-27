using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;

public class UseAllChatFeelings : DailyTaskProgressListener
{
    public enum Feeling
    {
        Sadness,
        Anger,
        Joy,
        Playful,
        Love
    }

    private Dictionary<Feeling, bool> feelingsUsed = new()
    {
        { Feeling.Sadness, false},
        { Feeling.Anger, false},
        { Feeling.Joy, false},
        { Feeling.Playful, false},
        { Feeling.Love, false}
    };

    private void Awake()
    {
        _educationCategoryType = EducationCategoryType.Social;
        _educationCategorySocialType = TaskEducationSocialType.UseAllChatFeelings;
    }

    public void FeelingUsed(Feeling feeling)
    {
        feelingsUsed[feeling] = true;

        bool allUsed = feelingsUsed.Values.All(v => v);

        if (allUsed)
        {
            UpdateProgress("1");
        }
    }
}
