using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MenuUi.Scripts.Window
{
    public class MainMenuNavigation : WindowNavigation
    {
        [SerializeField] protected WindowDef _alternativeNaviTarget;

        public override IEnumerator Navigate()
        {
            if (ServerManager.Instance.Clan == null)
            {
                yield return Navigate(_alternativeNaviTarget);
            }
            else
            {
                yield return base.Navigate();
            }
        }
#if UNITY_EDITOR
        [CustomEditor(typeof(MainMenuNavigation))]
        public class MainMenuNavigationEditor : WindowNavigationEditor
        {
            protected SerializedProperty AlternativeNaviTarget;

            protected override void OnEnable()
            {
                base.OnEnable();
                AlternativeNaviTarget = serializedObject.FindProperty(nameof(_alternativeNaviTarget));
            }

            public override void OnInspectorGUI()
            {
                MainMenuNavigation script = (MainMenuNavigation)target;

                serializedObject.Update();

                EditorGUILayout.PropertyField(NaviTarget);
                EditorGUILayout.PropertyField(AlternativeNaviTarget);
                EditorGUILayout.PropertyField(UseNonDefaultWindow);
                if (script._useNonDefaultWindow != false) EditorGUILayout.PropertyField(TargetWindow);
                EditorGUILayout.PropertyField(IsCurrentPopOutWindow);

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
