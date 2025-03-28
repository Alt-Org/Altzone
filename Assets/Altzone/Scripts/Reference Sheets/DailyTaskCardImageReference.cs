using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/DailyTaskCardImageReference", fileName = "DailyTaskCardImageReference")]
    public class DailyTaskCardImageReference : ScriptableObject
    {
        [SerializeField] private List<NormalTaskCardImage> _normalTaskCardImages;
        [SerializeField] private List<EducationTaskCardImageBaseCategory> _educationTaskCardImages;

        [System.Serializable]
        public class NormalTaskCardImage
        {
            public string Name;
            public TaskNormalType Type;
            public Sprite Image;
        }

        [System.Serializable]
        public class EducationTaskCardImageBaseCategory //TODO: make editor code to show only one sub type.
        {
            public string Name;
            public EducationCategoryType BaseType;
            public List<EducationTaskCardImageSubCategory> SubCategories;
        }

        [System.Serializable]
        public class EducationTaskCardImageSubCategory //TODO: make editor code to show only one sub type.
        {
            public string Name;
            public TaskEducationSocialType SocialType;
            public TaskEducationStoryType StoryType;
            public TaskEducationCultureType CultureType;
            public TaskEducationEthicalType EthicalType;
            public TaskEducationActionType ActionType;
            public Sprite Image;
        }

        public Sprite GetTaskImage(PlayerTask data)
        {
            if (data.Type != TaskNormalType.Undefined)
                return(GetNormalTaskImage(data.Type));
            else
            {
                int subType = 0;

                switch (data.EducationCategory)
                {
                    case EducationCategoryType.Social: subType = (int)data.EducationSocialType; break;
                    case EducationCategoryType.Story: subType = (int)data.EducationStoryType; break;
                    case EducationCategoryType.Culture: subType = (int)data.EducationCultureType; break;
                    case EducationCategoryType.Ethical: subType = (int)data.EducationEthicalType; break;
                    case EducationCategoryType.Action: subType = (int)data.EducationActionType; break;
                    default: break;
                }

                 return(GetEducationTaskImage(data.EducationCategory, subType));
            }
        }

        private Sprite GetNormalTaskImage(TaskNormalType type)
        {
            foreach (NormalTaskCardImage data in _normalTaskCardImages)
                if (data.Type == type)
                    return (data.Image);

            return (null);
        }

        private Sprite GetEducationTaskImage(EducationCategoryType baseType, int subType)
        {
            foreach (EducationTaskCardImageBaseCategory baseData in _educationTaskCardImages)
                if (baseData.BaseType == baseType)
                    foreach (EducationTaskCardImageSubCategory subData in baseData.SubCategories)
                        switch (baseData.BaseType)
                        {
                            case EducationCategoryType.Social:
                                {
                                    if (subData.SocialType == (TaskEducationSocialType)subType)
                                        return (subData.Image);

                                    break;
                                }
                            case EducationCategoryType.Story:
                                {
                                    if (subData.StoryType == (TaskEducationStoryType)subType)
                                        return (subData.Image);

                                    break;
                                }
                            case EducationCategoryType.Culture:
                                {
                                    if (subData.CultureType == (TaskEducationCultureType)subType)
                                        return (subData.Image);

                                    break;
                                }
                            case EducationCategoryType.Ethical:
                                {
                                    if (subData.EthicalType == (TaskEducationEthicalType)subType)
                                        return (subData.Image);

                                    break;
                                }
                            case EducationCategoryType.Action:
                                {
                                    if (subData.ActionType == (TaskEducationActionType)subType)
                                        return (subData.Image);

                                    break;
                                }
                            default: break;
                        }

            return (null);
        }
    }
}
