using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Altzone.Scripts.Language
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextLanguageSelector : MonoBehaviour
    {
        [SerializeField, TextArea(1, 10)] protected string _finnishText;
        [SerializeField, TextArea(1, 10)] protected string _englishText;

        protected TMP_Text _textField;
        protected virtual void OnEnable()
        {
            _textField = GetComponent<TMP_Text>();
            SettingsCarrier.OnLanguageChanged += SetText;
            SetText(SettingsCarrier.Instance.Language);
        }

        protected virtual void SetText(SettingsCarrier.LanguageType language)
        {
            if(!_textField) _textField = GetComponent<TMP_Text>();
            if (language is SettingsCarrier.LanguageType.Finnish) _textField.text = _finnishText;
            else if (language is SettingsCarrier.LanguageType.English) _textField.text = _englishText;
            else _textField.text = _finnishText;
        }

        public void EditorTextSet(SettingsCarrier.LanguageType language)
        {
            SetText(language);
        }
    }

    [CustomEditor(typeof(TextLanguageSelector))]
    public class TextLanguageSelectorEditor : Editor
    {
        TextLanguageSelector _script;
        SerializedProperty _finnishText;
        SerializedProperty _englishText;

        public void OnEnable()
        {
            _script = target as TextLanguageSelector;
            _finnishText = serializedObject.FindProperty("_finnishText");
            _englishText = serializedObject.FindProperty("_englishText");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_finnishText);
            EditorGUILayout.PropertyField(_englishText);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                _script.EditorTextSet(SettingsCarrier.LanguageType.Finnish);
            }

        }
    }
}
