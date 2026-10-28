using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
            _textField = GetComponent<TMP_Text>();
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
                _script.EditorTextSet(LanguageType.Finnish);
            }
        }
    }
#endif
}
