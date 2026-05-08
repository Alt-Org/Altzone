#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(SwitchToggle), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the Toggle Component.
    /// Extend this class to write a custom editor for a component derived from Toggle.
    /// </summary>
    public class SwitchToggleEditor : ToggleEditor
    {
        SerializedProperty _mode;
        SerializedProperty _slider;
        SerializedProperty _background;
        SerializedProperty _fill;

        protected override void OnEnable()
        {
            base.OnEnable();

            _mode = serializedObject.FindProperty("_spriteMode");
            _slider = serializedObject.FindProperty("_togglesprite");
            _background = serializedObject.FindProperty("_activeSprite");
            _fill = serializedObject.FindProperty("_inactiveSprite");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            SwitchToggle script = (SwitchToggle)target;

            serializedObject.Update();
            EditorGUILayout.PropertyField(_mode);
            EditorGUILayout.PropertyField(_slider);
            if (script.Mode is SwitchToggle.SpriteMode.SpriteSwap)
            {
                EditorGUILayout.PropertyField(_background);
                EditorGUILayout.PropertyField(_fill);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
