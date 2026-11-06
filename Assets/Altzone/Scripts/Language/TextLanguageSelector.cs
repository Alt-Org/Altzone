using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using ReadOnlyAttribute = Unity.Collections.ReadOnlyAttribute;

namespace Altzone.Scripts.Language
{
    [RequireComponent(typeof(TMP_Text)), ExecuteAlways]
    public class TextLanguageSelector : MonoBehaviour
    {
        [SerializeField, ReadOnly] protected TMP_Text _textField;

        [SerializeField, TextArea(1, 10)] protected string _finnishText;
        [SerializeField, TextArea(1, 10)] protected string _englishText;

        private bool _textFieldFound = false;

        public bool TextFieldFound { get => _textFieldFound;}

        protected virtual void OnEnable()
        {
            GetText();
            Localization.OnLanguageChanged += SetText;
            SetText(Localization.Language);
        }

        private void OnDisable()
        {
            Localization.OnLanguageChanged -= SetText;
        }

        public void GetText()
        {
            TMP_Text textField = GetComponent<TMP_Text>();
            if (textField)
            {
                _textField = textField;
                _textFieldFound = true;
            }
            else _textFieldFound = false;
        }

        protected virtual void SetText(LanguageType language)
        {
            if(!_textField) GetText();
            if (language is LanguageType.Finnish) _textField.text = _finnishText;
            else if (language is LanguageType.English) _textField.text = _englishText;
            else _textField.text = _finnishText;
        }

        public void EditorTextSet(LanguageType language)
        {
            SetText(language);
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(TextLanguageSelector), true)]
    public class TextLanguageSelectorEditor : Editor
    {
        TextLanguageSelector _script;
        SerializedProperty _finnishText;
        SerializedProperty _englishText;
        SerializedProperty _textField;

        public void OnEnable()
        {
            _script = target as TextLanguageSelector;
            _finnishText = serializedObject.FindProperty("_finnishText");
            _englishText = serializedObject.FindProperty("_englishText");
            _textField = serializedObject.FindProperty("_textField");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            if (_script.TextFieldFound)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_textField);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.PropertyField(_textField);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_finnishText);
            EditorGUILayout.PropertyField(_englishText);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                _script.EditorTextSet(Localization.Language);
            }
        }
    }
#endif
}
