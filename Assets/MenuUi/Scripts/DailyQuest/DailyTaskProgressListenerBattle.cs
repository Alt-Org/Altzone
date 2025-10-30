using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;

public class DailyTaskProgressListenerBattle : DailyTaskProgressListener
{
    public void PlayBattle()
    {
        _educationCategoryType = EducationCategoryType.Action;
        _educationCategoryActionType = TaskEducationActionType.PlayBattle;
        UpdateProgress("1");
    }

    public void WinBattle()
    {
        _educationCategoryType = EducationCategoryType.Action;
        _educationCategoryActionType = TaskEducationActionType.WinBattle;
        UpdateProgress("1");
    }
}
