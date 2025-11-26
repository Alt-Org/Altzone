using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public class DailyTaskProgressListenerCharacterStats : DailyTaskProgressListener
{
    public void UpdateProgressByStatType(StatType statType)
    {
        _educationCategoryType = EducationCategoryType.Action;

        switch (statType)
        {
            case StatType.Speed:
                _educationCategoryActionType = TaskEducationActionType.MakeCharacterFast;
                break;
            case StatType.Hp:
                _educationCategoryActionType = TaskEducationActionType.MakeCharacterDurable;
                break;
            case StatType.Attack:
                _educationCategoryActionType = TaskEducationActionType.MakeCharacterStrong;
                break;
            case StatType.CharacterSize:
                _educationCategoryActionType = TaskEducationActionType.MakeCharacterBig;
                break;
        }

        UpdateProgress("1");
    }
}
