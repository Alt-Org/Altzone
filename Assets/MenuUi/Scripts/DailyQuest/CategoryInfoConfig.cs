using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CategoryInfo
{
    public string Id
    {
        get
        {
            return _id;
        }
    }

    public string Name
    {
        get
        {
            return SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English
                ? _englishName : _name;
        }
    }

    public Color Color
    {
        get
        {
            return _color;
        }
    }

    public string Title
    {
        get
        {
            return SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English
                ? _englishTitle : _title;
        }
    }

    public string Description
    {
        get
        {
            return SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English
                ? _englishDescription : _description;

        }
    }

    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [SerializeField] private string _englishName;
    [SerializeField] private Color _color;

    [SerializeField] private string _title;
    [SerializeField] private string _englishTitle;
    [SerializeField] private string _description;
    [SerializeField] private string _englishDescription;
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
