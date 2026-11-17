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

    [SerializeField] LocalizationBlock _localizationBlock = null;
    [SerializeField] LocalizationBlock _localizationBlock2 = null;

    private static Localization _instance;
    private static bool _hasInstance;

    public delegate void LanguageChanged(LanguageType language);
    public static event LanguageChanged OnLanguageChanged;

    public delegate void SaveChangesEvent();
    public event SaveChangesEvent OnSaveChanges;

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
    public LocalizationBlock LocalizationBlock2 { get => _localizationBlock2; }

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

        _localizationBlock = new(0, _languageList);
        _localizationBlock2 = new(1, _languageList);
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

    public void SaveChanges()
    {
        OnSaveChanges?.Invoke();
#if UNITY_EDITOR
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
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

    public void UpdateSelections(bool active)
    {
        _selected = active;
        Localization.Instance.SaveChanges();
    }
}

[Serializable]
public class LocalizationBlock
{
    [SerializeField] private int _id;
    [SerializeField] private string _key;
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
    public string Key { get => _key; set => _key = value; }
    public int Id { get => _id; }

    public LocalizationBlock(int blockid, List<LanguageSelector> LanguageList)
    {
        _id = blockid;
        _languageBlock = new();
        int i = 0;
        foreach (LanguageSelector selector in LanguageList)
        {
            _languageBlock.Add(new(blockid*100+i,selector, string.Empty));
            i++;
        }
    }

    public void UpdateBlock()
    {
        OnBlockUpdate?.Invoke();
    }
}

[Serializable]
public class LocalizationLanguageBlock
{
    [SerializeField] private int _id;
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

    public int Id { get => _id;}

    public delegate void UpdateLocalizationText();
    public static event UpdateLocalizationText OnUpdateLocalizationText;

    public LocalizationLanguageBlock(int id,LanguageSelector type, string text)
    {
        _id = id;
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
    VisualElement _blockBoxElement;
    BlockVisualElement _localizationBlockElement;
    LocalizationBlock _localizationBlock;
    BlockVisualElement _localizationBlockElement2;
    LocalizationBlock _localizationBlock2;
    PopupField<LanguageType> _popup;

    public delegate void UpdateLanguageList();
    public static event UpdateLanguageList OnUpdateLanguageList;

    public void OnEnable()
    {
        _script = target as Localization;
        _language = serializedObject.FindProperty("_language");
        _languageList = serializedObject.FindProperty("_languageList");
        _localizationBlock = _script.LocalizationBlock;
        _localizationBlock2 = _script.LocalizationBlock2;
        serializedObject.Update();
        _script.Initialize();
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        LanguageSelectorEditor.OnUpdateLanguageList += UpdateLanguageSelectionListCall;
        Localization.Instance.OnSaveChanges += Save;
    }

    private void OnDisable()
    {
        LanguageSelectorEditor.OnUpdateLanguageList -= UpdateLanguageSelectionListCall;
        if(OnUpdateLanguageList != null && OnUpdateLanguageList?.GetInvocationList() != null)
        foreach (Delegate d in OnUpdateLanguageList.GetInvocationList())
        {
            OnUpdateLanguageList -= (UpdateLanguageList)d;
        }
        Localization.Instance.OnSaveChanges -= Save;
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

        _blockBoxElement = new Box();

        _blockBoxElement.style.minWidth = 500f;
        _blockBoxElement.style.flexGrow = 1;
        _blockBoxElement.style.backgroundColor = new Color(0.3f,0.3f,0.3f);
        _blockBoxElement.style.flexDirection = FlexDirection.Column;
        _blockBoxElement.style.flexWrap = Wrap.Wrap;

        container.Add(_blockBoxElement);

        Debug.LogWarning("0");

        _localizationBlock = Localization.Instance.LocalizationBlock;
        _localizationBlockElement = new(_localizationBlock);
        _blockBoxElement.Add(_localizationBlockElement);

        _localizationBlock2 = Localization.Instance.LocalizationBlock2;
        _localizationBlockElement2 = new(_localizationBlock2);
        _blockBoxElement.Add(_localizationBlockElement2);

        Debug.LogWarning(Localization.Instance.LocalizationBlock.Key);
        Debug.LogWarning(Localization.Instance.LocalizationBlock2.Key);

        //PropertyField localizationField = new(_localizationBlockProperty);
        //container.Add(localizationField);

        //PropertyField localizationField2 = new(_localizationBlock2Property);
        //container.Add(localizationField2);

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
        _script.LanguageList.FirstOrDefault(x => x.Language == language).UpdateSelections(active);
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
        foreach (LocalizationLanguageBlock block in _script.LocalizationBlock2.LanguageBlock)
        {
            if (block.Language == language)
                block.Active = active;
        }
        serializedObject.ApplyModifiedProperties();
        OnUpdateLanguageList?.Invoke();
        //_script.LocalizationBlock.UpdateBlock();
        Debug.LogWarning("Update5?");
        serializedObject.Update();
    }

    public void Save()
    {
        serializedObject.ApplyModifiedProperties();
        SaveChanges();
        //serializedObject.Update();
        Debug.LogWarning("Saved?");
        Debug.LogWarning(Localization.Instance.LocalizationBlock.Key);
        Debug.LogWarning(Localization.Instance.LocalizationBlock2.Key);
    }
}

[CustomPropertyDrawer(typeof(LocalizationBlock))]
public class LocalizationBlockEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        BlockVisualElement content = new BlockVisualElement(property.boxedValue as LocalizationBlock);
        return content;
    }

    
}

[Serializable]
public class BlockVisualElement : VisualElement
{
    List<LocalizationLanguageBlock> _languageBlockList = new();
    string _keyProperty;
    List<TextField> _languageBlockFieldList = new();
    Foldout _languageBlockListField;
    LocalizationBlock _script;
    int _id;

    public BlockVisualElement(LocalizationBlock property) : base()
    {
        if (_script == null)
        {
            _script = property;
            _id = _script.Id;
            Debug.LogWarning("Id: " + _id);
        }
        LocalizationEditor.OnUpdateLanguageList += UpdateBlockList;
        //_script.OnBlockUpdate += (() => UpdateBlockList());

        _keyProperty = _script.Key;
        foreach (LocalizationLanguageBlock block in _script.LanguageBlock)
        {
            _languageBlockList.Add(block);
        }

        UpdateBlockList();
    }

    public void UpdateBlockList()
    {
        Foldout languageBlockListField;
        if (_languageBlockListField == null)
        {
            languageBlockListField = new Foldout();
            TextField text = new("Key");
            text.value = _script.Key;
            text.RegisterCallback<ChangeEvent<string>>(type =>
            {
                _script.Key = type.newValue;
                languageBlockListField.text = _script.Key;
                Localization.Instance.SaveChanges();
            });

            Add(text);
            languageBlockListField.text = _script.Key;
            foreach (LocalizationLanguageBlock block in _languageBlockList)
            {
                TextField localizationText = new(block.Language.ToString());
                languageBlockListField.Add(localizationText);
                _languageBlockFieldList.Add(localizationText);
                if (block.Active)
                {
                    localizationText.style.display = DisplayStyle.Flex;
                }
                else
                {
                    localizationText.style.display = DisplayStyle.None;
                }
            }
            Add(languageBlockListField);
        }
        else
        {
            languageBlockListField = _languageBlockListField;
            int i = 0;
            foreach (LocalizationLanguageBlock block in _languageBlockList)
            {
                TextField localizationText = _languageBlockFieldList[i++];
                if (block.Active)
                {
                    localizationText.style.display = DisplayStyle.Flex;
                }
                else
                {
                    localizationText.style.display = DisplayStyle.None;
                }
            }
        }
        _languageBlockListField = languageBlockListField;
        return;
    }
}

[CustomPropertyDrawer(typeof(LocalizationLanguageBlock))]
public class LocalizationLanguageBlockEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new BlockPieceVisualElement(property);
        return container;
    }

    public class BlockPieceVisualElement : VisualElement
    {
        private LocalizationLanguageBlock _block;

        public LocalizationLanguageBlock Block => _block;
        public BlockPieceVisualElement(SerializedProperty property)
        {
            _block = (property.boxedValue as LocalizationLanguageBlock);

            var languageProperty = property.FindPropertyRelative("_language");
            var textProperty = property.FindPropertyRelative("_text");
            var textField = new PropertyField(textProperty, ((LanguageType)languageProperty.enumValueIndex).ToString());

            textField.RegisterCallback<ChangeEvent<bool>>(type =>
            {
                LocalizationLanguageBlock selector = property.boxedValue as LocalizationLanguageBlock;
                selector.UpdateSelection();
            });

            Add(textField);

        }
    }
}

[CustomPropertyDrawer(typeof(LanguageSelector))]
public class LanguageSelectorEditor : PropertyDrawer
{
    public delegate void UpdateLanguageList(LanguageType language, bool value);
    public static event UpdateLanguageList OnUpdateLanguageList;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        var languageProperty = property.FindPropertyRelative("_language");
        var selectedProperty = property.FindPropertyRelative("_selected");

        Toggle selectedField = new(((LanguageType)languageProperty.enumValueIndex).ToString());
        selectedField.SetValueWithoutNotify((property.boxedValue as LanguageSelector).Selected);

        selectedField.RegisterCallback<ChangeEvent<bool>>(type =>
        {
            Debug.LogWarning(type.newValue);
            LanguageSelector selector = property.boxedValue as LanguageSelector;
            //selector.UpdateSelection(type.newValue);
            OnUpdateLanguageList.Invoke(selector.Language, type.newValue);
        });

        container.Add(selectedField);

        return container;
    }
}

#endif
