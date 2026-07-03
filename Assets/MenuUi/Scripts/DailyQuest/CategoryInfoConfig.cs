using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CategoryInfo
{

    [HideInInspector]
    public string Name
    {
        get
        {
            if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
            {
                return englishName;
            }
            else
            {
                return name;
            }

        }
    }

    [HideInInspector]
    public Color Color
    {
        get
        {
            return color;
        }
    }

    [HideInInspector]
    public string Title
    {
        get
        {
            if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
            {
                return englishTitle;
            }
            else
            {
                return title;
            }
                
        }
    }

    [HideInInspector]
    public string Description
    {
        get
        {
            if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
            {
                return englishDescription;
            }
            else
            {
                return description;
            }

        }
    }
    [SerializeField] private string name;
    [SerializeField] private string englishName;
    [SerializeField] private Color color;

    [SerializeField] private string title;
    [SerializeField] private string englishTitle;
    [SerializeField] private string description;
    [SerializeField] private string englishDescription;

}

[CreateAssetMenu(fileName = "NewCategoryInfoConfig", menuName = "ALT-Zone/DailyTask/CategoryInfoConfig")]
public class CategoryInfoConfig : ScriptableObject
{

    private static CategoryInfoConfig _instance;

    public static CategoryInfoConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CategoryInfoConfig>(nameof(CategoryInfoConfig));
            }

            return _instance;
        }
    }

    [SerializeField] private List<CategoryInfo> _categories;

    public List<CategoryInfo> GetCategoryInfos() => _categories;



}
