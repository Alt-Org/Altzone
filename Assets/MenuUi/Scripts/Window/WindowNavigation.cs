using System.Collections;
using Altzone.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MenuUi.Scripts.Window
{
    public class WindowNavigation : MonoBehaviour
    {
        private const string Tooltip = "Pop out and hide current window before showing target window";

        [Header("Settings"), SerializeField] protected WindowDef _naviTarget;
        [SerializeField] protected bool _useNonDefaultWindow = false;
        [SerializeField] protected int _targetWindow;
        [Tooltip(Tooltip), SerializeField] protected bool _isCurrentPopOutWindow;

        public WindowDef NaviTarget { get => _naviTarget; }

        virtual public IEnumerator Navigate()
        {
            yield return Navigate(_naviTarget);

        }

        protected IEnumerator Navigate(WindowDef naviTarget)
        {
            Debug.Log($"naviTarget {naviTarget} isCurrentPopOutWindow {_isCurrentPopOutWindow}", naviTarget);
            var windowManager = WindowManager.Get();
            yield return new WaitUntil(() => windowManager.ExecutionLevel == 0);
            if (_isCurrentPopOutWindow)
            {
                windowManager.PopCurrentWindow();
            }
            // Check if navigation target window is already in window stack and we area actually going back to it via button.
            var windowCount = windowManager.WindowCount;
            if (windowCount > 1)
            {
                var targetIndex = windowManager.FindIndex(naviTarget);
                if (targetIndex == 1)
                {
                    windowManager.GoBack();
                    yield break;
                }
                if (targetIndex > 1)
                {
                    windowManager.Unwind(naviTarget);
                    windowManager.GoBack();
                    yield break;
                }
            }
            if (_useNonDefaultWindow == true)
                DataCarrier.AddData(DataCarrier.RequestedWindow, _targetWindow);
            windowManager.ShowWindow(naviTarget);
        }
#if UNITY_EDITOR
        [CustomEditor(typeof(WindowNavigation))]
        public class WindowNavigationEditor : Editor
        {
            protected SerializedProperty NaviTarget;
            protected SerializedProperty UseNonDefaultWindow;
            protected SerializedProperty TargetWindow;
            protected SerializedProperty IsCurrentPopOutWindow;

            protected virtual void OnEnable()
            {
                NaviTarget = serializedObject.FindProperty(nameof(_naviTarget));
                UseNonDefaultWindow = serializedObject.FindProperty(nameof(_useNonDefaultWindow));
                TargetWindow = serializedObject.FindProperty(nameof(_targetWindow));
                IsCurrentPopOutWindow = serializedObject.FindProperty(nameof(_isCurrentPopOutWindow));
            }

            public override void OnInspectorGUI()
            {
                WindowNavigation script = (WindowNavigation)target;

                serializedObject.Update();

                EditorGUILayout.PropertyField(NaviTarget);
                EditorGUILayout.PropertyField(UseNonDefaultWindow);
                if(script._useNonDefaultWindow != false) EditorGUILayout.PropertyField(TargetWindow);
                EditorGUILayout.PropertyField(IsCurrentPopOutWindow);

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
