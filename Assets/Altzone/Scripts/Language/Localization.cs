using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "ALT-Zone/LocalizationSheet", fileName = "LocalizationSheet")]
public class Localization : ScriptableObject
{
    [SerializeField] LanguageType _language = LanguageType.None;

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

    public void SaveLanguage()
    {
        PlayerPrefs.SetString("LanguageType", ParseLanguage(_language));
        OnLanguageChanged?.Invoke(_language);
    }

    private void Initialize()
    {
        _language = ParseLanguage(PlayerPrefs.GetString("LanguageType", ""));
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

#if UNITY_EDITOR
[CustomEditor(typeof(Localization), true)]
    public class LocalizationEditor : Editor
{
    Localization _script;
    SerializedProperty _language;

    public void OnEnable()
    {
        _script = target as Localization;
        _language = serializedObject.FindProperty("_language");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_language);

        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            _script.SaveLanguage();
        }
    }
}
#endif
