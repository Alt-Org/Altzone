using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Language;
using Newtonsoft.Json.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "ALT-Zone/LocalizationSheet", fileName = "LocalizationSheet")]
public class Localization : ScriptableObject
{
    [SerializeField] LanguageType _language = LanguageType.None;

    [SerializeField] List<LanguageSelector> _languageList;

    private static Localization _instance;
    private static bool _hasInstance;

    public delegate void LanguageChanged(LanguageType language);
    public static event LanguageChanged OnLanguageChanged;

    public static Localization Instance
    {
        get
        {
            if (!_hasInstance)
            {
                _instance = Resources.Load<Localization>(nameof(Localization));
                _hasInstance = _instance != null;
                _instance.Initialize();
            }
            return _instance;
        }

    }

    public static LanguageType Language
    {
        get => Instance.LanguageType;
        set => Instance.LanguageType = value;
    }

    private LanguageType LanguageType
    {
        get { return _language; }
        set
        {
            if (value == _language) return;
            _language = value;
            SaveLanguage();
        }
    }

    public List<LanguageSelector> LanguageList { get => _languageList;}

    public void SaveLanguage()
    {
        PlayerPrefs.SetString("LanguageType", ParseLanguage(_language));
        OnLanguageChanged?.Invoke(_language);
    }

    public void Initialize()
    {
        _language = ParseLanguage(PlayerPrefs.GetString("LanguageType", ""));
        List<LanguageType> enumList = Enum.GetValues(typeof(LanguageType)).Cast<LanguageType>().ToList();
        enumList.Remove(LanguageType.None);
        if (_languageList == null || _languageList.Count <=0)
        {
            _languageList = new();
            foreach (LanguageType type in enumList)
            {
                _languageList.Add(new(type));
            }
        }
        else
        {
            List<LanguageSelector> checkedList = new();
            List<LanguageSelector> remainingList = new(_languageList);

            foreach (LanguageType type in enumList)
            {
                LanguageSelector selector = remainingList.FirstOrDefault(x => x.Language == type);
                if (selector != null)
                {
                    checkedList.Add(selector);
                    remainingList.Remove(selector);
                }
                else
                {
                    checkedList.Add(new(type));
                }
            }
            _languageList = checkedList;
        }
    }

    private LanguageType ParseLanguage(string languageName)
    {
        return languageName switch
        {
            "fi" => LanguageType.Finnish,
            "en" => LanguageType.English,
            _ => LanguageType.None,
        };
    }

    private string ParseLanguage(LanguageType languageType)
    {
        return languageType switch
        {
            LanguageType.Finnish => "fi",
            LanguageType.English => "en",
            _ => "",
        };
    }

}

public enum LanguageType
{
    None,
    Finnish,
    English,
    Swedish
}

[Serializable]
public class LanguageSelector
{
    [SerializeField] private LanguageType _language;
    [SerializeField] private bool _selected;

    public LanguageType Language => _language;
    public bool Selected => _selected;

    public LanguageSelector(LanguageType type)
    {
        _language = type;
        _selected = true;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Localization), true)]
public class LocalizationEditor : Editor
{
    Localization _script;
    SerializedProperty _language;
    SerializedProperty _languageList;

    public void OnEnable()
    {
        _script = target as Localization;
        _language = serializedObject.FindProperty("_language");
        _languageList = serializedObject.FindProperty("_languageList");
        serializedObject.Update();
        _script.Initialize();
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
    }

    public override VisualElement CreateInspectorGUI()
    {
        serializedObject.Update();

        // Create property container element.
        var container = new VisualElement();

        PropertyField scriptField = new PropertyField(serializedObject.FindProperty("m_Script"));
        scriptField.SetEnabled(false);
        container.Add(scriptField);

        PopupField<LanguageType> popup = new()
        {
            label = _language.displayName
        };

        List<LanguageSelector> selectorList = _script.LanguageList.Where(x => x.Selected).ToList();

        foreach (LanguageSelector type in selectorList)
        {
            popup.choices.Add(type.Language);
        }
        popup.value = (LanguageType)_language.enumValueIndex;

        popup.RegisterCallback<ChangeEvent<LanguageType>>(type =>
        {
            _language.enumValueIndex = (int)type.newValue;
            serializedObject.ApplyModifiedProperties();
            _script.SaveLanguage();
        });

        container.Add(popup);

        PropertyField languageList = new(_languageList);
        container.Add(languageList);

        return container;
    }
}

[CustomPropertyDrawer(typeof(LanguageSelector))]
public class LanguageSelectorEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        var languageProperty = property.FindPropertyRelative("_language");
        var selectedProperty = property.FindPropertyRelative("_selected");
        var selectedField = new PropertyField(selectedProperty, ((LanguageType)languageProperty.enumValueIndex).ToString());

        container.Add(selectedField);

        return container;
    }
}

#endif
