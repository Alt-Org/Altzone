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

        if (_localizationBlock == null) _localizationBlock = new(0, _languageList);

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

    public void UpdateSelection(bool selected)
    {
        _selected = selected;
        OnUpdateLanguageList?.Invoke(_language, _selected);
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

    public LocalizationBlock(int blockid,List<LanguageSelector> LanguageList)
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
        Debug.LogWarning("Update1?");
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
        if(OnUpdateLanguageList != null && OnUpdateLanguageList?.GetInvocationList() != null)
        foreach (Delegate d in OnUpdateLanguageList.GetInvocationList())
        {
            OnUpdateLanguageList -= (UpdateLanguageList)d;
        }
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

        PropertyField localizationField = new(_localizationBlock);
        container.Add(localizationField);

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
        //_script.LocalizationBlock.UpdateBlock();
        Debug.LogWarning("Update5?");
        serializedObject.Update();
    }

}

[CustomPropertyDrawer(typeof(LocalizationBlock))]
public class LocalizationBlockEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        Debug.LogWarning("Update0?");
        BlockVisualElement content = new BlockVisualElement(property);
        return content;
    }

    [Serializable]
    public class BlockVisualElement : VisualElement
    {
        List<LocalizationLanguageBlock> _languageBlockList = new();
        string _keyProperty;
        List<TextField> _languageBlockFieldList = new();
        Foldout _languageBlockListField;
        LocalizationBlock _script;
        int id;

        public BlockVisualElement(SerializedProperty property): base()
        {
            if (_script == null)
            {
                _script = property.boxedValue as LocalizationBlock;
                id = _script.Id;
                Debug.LogWarning("Id: "+id);
                _script = Localization.Instance.GetBlock(id);
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
            Debug.LogWarning("Update2?");
            if (_languageBlockListField == null)
            {
                languageBlockListField = new Foldout();
                TextField text = new TextField("Key");
                text.value = _script.Key;
                text.RegisterCallback<ChangeEvent<string>>(type =>
                {
                    _script.Key = type.newValue;
                    languageBlockListField.text = _script.Key;
                    //serializedObject.ApplyModifiedProperties();
                });

                Add(text);
                languageBlockListField.text = _script.Key;
                foreach (LocalizationLanguageBlock block in _languageBlockList)
                {
                    //PropertyField field = new PropertyField(block);
                    TextField localizationText = new TextField(block.Language.ToString());
                    languageBlockListField.Add(localizationText);
                    _languageBlockFieldList.Add(localizationText);
                    Debug.LogWarning(block.Language + " : " + block.Active);
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
                //Clear();
                languageBlockListField = _languageBlockListField;
                int i = 0;
                foreach (LocalizationLanguageBlock block in _languageBlockList)
                {
                    TextField localizationText = _languageBlockFieldList[i++];
                    Debug.LogWarning(block.Language+" : "+ block.Active);
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
}

[CustomPropertyDrawer(typeof(LocalizationLanguageBlock))]
public class LocalizationLanguageBlockEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new BlockPieceVisualElement(property);
        Debug.LogWarning("Update10?");
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
            selector.UpdateSelection(type.newValue);
        });

        container.Add(selectedField);

        return container;
    }
}

#endif
