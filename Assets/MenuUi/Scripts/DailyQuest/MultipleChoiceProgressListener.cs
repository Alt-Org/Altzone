using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public class MultipleChoiceProgressListener : DailyTaskProgressListener
{
    public void UpdateProgressMultipleChoice(PlayerTask task)
    {
        _normalTaskType = task.Type;
        _educationCategoryType = task.EducationCategory;
        switch (task.EducationCategory)
        {
            case EducationCategoryType.Social: _educationCategorySocialType = task.EducationSocialType; break;
            case EducationCategoryType.Story: _educationCategoryStoryType = task.EducationStoryType; break;
            case EducationCategoryType.Culture: _educationCategoryCultureType = task.EducationCultureType; break;
            case EducationCategoryType.Ethical: _educationCategoryEthicalType = task.EducationEthicalType; break;
            case EducationCategoryType.Action: _educationCategoryActionType = task.EducationActionType; break;
            default: break;
        }
        
        UpdateProgress("1");
    }
}
