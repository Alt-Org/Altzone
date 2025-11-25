using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(SliderToggle), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the Toggle Component.
    /// Extend this class to write a custom editor for a component derived from Toggle.
    /// </summary>
    public class SliderToggleEditor : ToggleEditor
    {
        SerializedProperty _slider;
        SerializedProperty _background;
        SerializedProperty _fill;
        SerializedProperty _handle;
        SerializedProperty _oncolour;
        SerializedProperty _offcolour;

        protected override void OnEnable()
        {
            base.OnEnable();

            _slider = serializedObject.FindProperty("_sliderSwitch");
            _background = serializedObject.FindProperty("_background");
            _fill = serializedObject.FindProperty("_fill");
            _handle = serializedObject.FindProperty("_handle");
            _oncolour = serializedObject.FindProperty("_oncolour");
            _offcolour = serializedObject.FindProperty("_offcolour");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_slider);
            EditorGUILayout.PropertyField(_background);
            EditorGUILayout.PropertyField(_fill);
            EditorGUILayout.PropertyField(_handle);
            EditorGUILayout.PropertyField(_oncolour);
            EditorGUILayout.PropertyField(_offcolour);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
