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

    [SerializeField] LocalizationBlock _localizationBlock;

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
    public LocalizationBlock LocalizationBlock { get => _localizationBlock;}

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

        if(_localizationBlock == null) _localizationBlock = new();

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

    public delegate void UpdateLanguageList(LanguageType type, bool active);
    public static event UpdateLanguageList OnUpdateLanguageList;

    public LanguageSelector(LanguageType type)
    {
        _language = type;
        _selected = true;
    }

    public void UpdateSelection()
    {
        OnUpdateLanguageList?.Invoke(_language, _selected);
    }
}

[Serializable]
public class LocalizationBlock
{
    [SerializeField] private List<LocalizationLanguageBlock> _languageBlock;

    public delegate void BlockUpdate();
    public event BlockUpdate OnBlockUpdate;

    public string LocalizedText
    {
        get
        { 
            return _languageBlock.FirstOrDefault(x => x.Language == Localization.Language).Text;
        }
    }

    public List<LocalizationLanguageBlock> LanguageBlock { get => _languageBlock;}

    public LocalizationBlock()
    {
        _languageBlock = new();
        foreach (LanguageSelector selector in Localization.Instance.LanguageList)
        {
            _languageBlock.Add(new(selector, string.Empty));
        }
    }

    public void UpdateBlock()
    {
        Debug.LogWarning("Update1?");
        OnBlockUpdate?.Invoke();
    }
}

[Serializable]
public class LocalizationLanguageBlock
{
    [SerializeField] private LanguageType _language;
    [SerializeField] private bool _active;
    [SerializeField] private string _text;

    public LanguageType Language => _language;
    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
        }
    }
    public string Text  => _text;

    public delegate void UpdateLocalizationText();
    public static event UpdateLocalizationText OnUpdateLocalizationText;

    public LocalizationLanguageBlock(LanguageSelector type, string text)
    {
        _language = type.Language;
        _active = type.Selected;
        _text = text;
    }

    public void UpdateSelection()
    {
        OnUpdateLocalizationText?.Invoke();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Localization), true)]
public class LocalizationEditor : Editor
{
    Localization _script;
    SerializedProperty _language;
    SerializedProperty _languageList;
    SerializedProperty _localizationBlock;
    PopupField<LanguageType> _popup;

    public delegate void UpdateLanguageList();
    public static event UpdateLanguageList OnUpdateLanguageList;

    public void OnEnable()
    {
        _script = target as Localization;
        _language = serializedObject.FindProperty("_language");
        _languageList = serializedObject.FindProperty("_languageList");
        _localizationBlock = serializedObject.FindProperty("_localizationBlock");
        serializedObject.Update();
        _script.Initialize();
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        LanguageSelector.OnUpdateLanguageList += UpdateLanguageSelectionListCall;
    }

    private void OnDisable()
    {
        LanguageSelector.OnUpdateLanguageList -= UpdateLanguageSelectionListCall;
    }

    public override VisualElement CreateInspectorGUI()
    {
        serializedObject.Update();

        // Create property container element.
        var container = new VisualElement();

        PropertyField scriptField = new PropertyField(serializedObject.FindProperty("m_Script"));
        scriptField.SetEnabled(false);
        container.Add(scriptField);

        PopupField<LanguageType> popup = UpdateLanguageSelectionList();

        container.Add(popup);

        PropertyField languageList = new(_languageList);
        container.Add(languageList);

        PropertyField localizationBlock = new(_localizationBlock);
        container.Add(localizationBlock);

        return container;
    }

    private PopupField<LanguageType> UpdateLanguageSelectionList()
    {
        if (_popup == null)
        {
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

            _popup ??= popup;

            return popup;
        }
        else
        {
            List<LanguageSelector> selectorList = _script.LanguageList.Where(x => x.Selected).ToList();

            _popup.choices.Clear();

            foreach (LanguageSelector type in selectorList)
            {
                _popup.choices.Add(type.Language);
            }

            if(!_popup.choices.Contains((LanguageType)_language.enumValueIndex))
            {
                if (_popup.choices.Count > 0)
                {
                    _popup.value = _popup.choices[0];
                    _language.enumValueIndex = (int)_popup.value;
                }
            }

            return _popup;
        }
    }

    private void UpdateLanguageSelectionListCall(LanguageType language, bool active)
    {
        UpdateLanguageSelectionList();
        UpdateLocalizationFields(language, active);
    }

    private void UpdateLocalizationFields(LanguageType language, bool active)
    {
        Debug.LogWarning(language+": "+active);
        foreach (LocalizationLanguageBlock block in _script.LocalizationBlock.LanguageBlock)
        {
            if (block.Language == language)
                block.Active = active;
        }
        serializedObject.ApplyModifiedProperties();
        OnUpdateLanguageList?.Invoke();
    }

}

[CustomPropertyDrawer(typeof(LocalizationBlock))]
public class LocalizationBlockEditor : PropertyDrawer
{
    SerializedProperty _languageBlockListProperty;
    Foldout _languageBlockListField;
    LocalizationBlock _script;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        if(_script == null)
        {
            _script = property.boxedValue as LocalizationBlock;
        }
        LocalizationEditor.OnUpdateLanguageList += UpdateBlockList;

        // Create property container element.
        var container = new VisualElement();

        _languageBlockListProperty = property.FindPropertyRelative("_languageBlock");

        UpdateBlockList();

        //var languageBlockListField = new PropertyField(languageBlockListProperty, "Text");


        container.Add(_languageBlockListField);

        

        return container;
    }

    public void UpdateBlockList()
    {
        Debug.LogWarning("Update2?");
        if (_languageBlockListField == null)
        {
            _languageBlockListField = new Foldout();
        }
        else
        {
            _languageBlockListField.Clear();
        }

        foreach (SerializedProperty block in _languageBlockListProperty)
        {
            Debug.LogWarning((block.boxedValue as LocalizationLanguageBlock).Language + ": " + (block.boxedValue as LocalizationLanguageBlock).Active);
            if ((block.boxedValue as LocalizationLanguageBlock).Active)
                _languageBlockListField.Add(new PropertyField(block));
        }
        Debug.LogWarning(_languageBlockListField.childCount);
    }
}

[CustomPropertyDrawer(typeof(LocalizationLanguageBlock))]
public class LocalizationLanguageBlockEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        var languageProperty = property.FindPropertyRelative("_language");
        var textProperty = property.FindPropertyRelative("_text");
        var textField = new PropertyField(textProperty, ((LanguageType)languageProperty.enumValueIndex).ToString());

        textField.RegisterCallback<ChangeEvent<bool>>(type =>
        {
            LocalizationLanguageBlock selector = property.boxedValue as LocalizationLanguageBlock;
            selector.UpdateSelection();
        });

        container.Add(textField);

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

        selectedField.RegisterCallback<ChangeEvent<bool>>(type =>
        {
            LanguageSelector selector = property.boxedValue as LanguageSelector;
            selector.UpdateSelection();
        });

        container.Add(selectedField);

        return container;
    }
}

#endif
