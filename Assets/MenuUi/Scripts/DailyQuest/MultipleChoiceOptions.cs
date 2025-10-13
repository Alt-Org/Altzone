using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;

//[CreateAssetMenu(fileName = "MultipleChoiceOptions")]
public class MultipleChoiceOptions : ScriptableObject
{
    private static MultipleChoiceOptions _instance;

    public static MultipleChoiceOptions Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<MultipleChoiceOptions>(nameof(MultipleChoiceOptions));
            }

            return _instance;
        }
    }

    [SerializeField] private List<NormalMultipleChoiceData> _normalTaskOptions;
    [SerializeField] private List<EducationMultipleChoiceData> _educationTaskOptions;

    [System.Serializable]
    public class NormalMultipleChoiceData
    {
        public string Name;
        public TaskNormalType Type;
        public List<OptionData> Options;
    }

    [System.Serializable]
    public class EducationMultipleChoiceData
    {
        public string Name;
        public EducationCategoryType Category;
        public TaskEducationActionType ActionType;
        public TaskEducationSocialType SocialType;
        public TaskEducationStoryType StoryType;
        public TaskEducationCultureType CultureType;
        public TaskEducationEthicalType EthicalType;
        public List<OptionData> Options;
    }

    [System.Serializable]
    public class OptionData
    {
        public string OptionText;
        public bool Result;
    }

    public bool IsMultipleChoice(PlayerTask data)
    {
        if (data.Type != TaskNormalType.Undefined)
        {
            var task = GetNormalMultipleChoiceData(data.Type);
            return (task == null) ? false : true;
        }
        else
        {
            var task = GetEducationMultipleChoiceData(data);
            return (task == null) ? false : true;
        }
    }

    public List<string> GetTaskOptions(PlayerTask data)
    {
        if (data.Type != TaskNormalType.Undefined)
        {
            var task = GetNormalMultipleChoiceData(data.Type);
            if (task == null) return null;
            return GetNormalTaskOptions(task);
        }
        else
        {
            var task = GetEducationMultipleChoiceData(data);
            if (task == null) return null;
            return GetEducationTaskOptions(task);
        }
    }

    private List<string> GetNormalTaskOptions(NormalMultipleChoiceData task)
    {
        List<string> options = new();
        foreach (var option in task.Options)
        {
            options.Add(option.OptionText);
        }

        return options;
    }

    private List<string> GetEducationTaskOptions(EducationMultipleChoiceData task)
    {
        List<string> options = new();
        foreach (var option in task.Options)
        {
            options.Add(option.OptionText);
        }

        return options;
    }

    public bool? GetResult(PlayerTask data, string answer)
    {
        if (data.Type != TaskNormalType.Undefined)
        {
            var task = GetNormalMultipleChoiceData(data.Type);
            if (task == null) return null;
            return GetNormalTaskResult(task, answer);
        }
        else
        {
            var task = GetEducationMultipleChoiceData(data);
            if (task == null) return null;
            return GetEducationTaskResult(task, answer);
        }
    }

    private bool GetNormalTaskResult(NormalMultipleChoiceData task, string answer)
    {
        OptionData optionData = null;
        foreach (var option in task.Options)
        {
            if (option.OptionText == answer)
            {
                optionData = option;
                break;
            }
        }

        return optionData.Result;
    }

    private bool GetEducationTaskResult(EducationMultipleChoiceData task, string answer)
    {
        OptionData optionData = null;
        foreach (var option in task.Options)
        {
            if (option.OptionText == answer)
            {
                optionData = option;
                break;
            }
        }

        return optionData.Result;
    }

    private NormalMultipleChoiceData GetNormalMultipleChoiceData(TaskNormalType type)
    {
        NormalMultipleChoiceData task = null;
        foreach (var normalTask in _normalTaskOptions)
        {
            if (normalTask.Type == type)
            {
                task = normalTask;
                break;
            }
        }
        return task;
    }

    private EducationMultipleChoiceData GetEducationMultipleChoiceData(PlayerTask data)
    {
        EducationMultipleChoiceData task = null;
        EducationCategoryType category = data.EducationCategory;
        foreach (var educationTask in _educationTaskOptions)
        {
            if (educationTask.Category == category)
            {
                switch (category)
                {
                    case EducationCategoryType.Social: if (educationTask.SocialType == data.EducationSocialType) task = educationTask; break;
                    case EducationCategoryType.Story: if (educationTask.StoryType == data.EducationStoryType) task = educationTask; break;
                    case EducationCategoryType.Culture: if (educationTask.CultureType == data.EducationCultureType) task = educationTask; break;
                    case EducationCategoryType.Ethical: if (educationTask.EthicalType == data.EducationEthicalType) task = educationTask; break;
                    case EducationCategoryType.Action: if (educationTask.ActionType == data.EducationActionType) task = educationTask; break;
                    default: break;
                }
            }
        }
        return task;
    }
}
