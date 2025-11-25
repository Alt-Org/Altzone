using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class VerticalScrollRect : BaseScrollRect
{
    public override bool HorizontallyScrollable
    {
        get { return false; }
    }

    public override Scrollbar HorizontalScrollbar
    {
        get { return null; }
    }

    public override ScrollbarVisibility HorizontalScrollbarVisibilityMode
    {
        get { return VerticalScrollbarVisibilityMode; }
    }

    public override float HorizontalScrollbarSpacing
    {
        get { return 0f; }
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Vertical Scroll Rect", false, 10)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/VerticalScrollView"));
        go.name = "VerticalScrollView";
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(VerticalScrollRect), true)]
[CanEditMultipleObjects]
/// <summary>
/// Custom Editor for the Toggle Component.
/// Extend this class to write a custom editor for a component derived from Toggle.
/// </summary>
public class VerticalScrollRectEditor : Editor
{
    SerializedProperty _content;
    SerializedProperty _scrollable;
    SerializedProperty _scrollingMovementType;
    SerializedProperty _elasticity;
    SerializedProperty _inertia;
    SerializedProperty _decelerationRate;
    SerializedProperty _scrollSensitivity;
    SerializedProperty _viewport;
    SerializedProperty _scrollbar;
    SerializedProperty _scrollbarVisibility;
    SerializedProperty _scrollSpacing;
    SerializedProperty _forwardEvents;
    SerializedProperty _onValueChanged;

    protected void OnEnable()
    {
        _content = serializedObject.FindProperty("content");
        _scrollable = serializedObject.FindProperty("verticallyScrollable");
        _scrollingMovementType = serializedObject.FindProperty("scrollingMovementType");
        _elasticity = serializedObject.FindProperty("elasticity");
        _inertia = serializedObject.FindProperty("inertia");
        _decelerationRate = serializedObject.FindProperty("decelerationRate");
        _scrollSensitivity = serializedObject.FindProperty("scrollSensitivity");
        _viewport = serializedObject.FindProperty("viewportRectTransform");
        _scrollbar = serializedObject.FindProperty("verticalScrollbar");
        _scrollbarVisibility = serializedObject.FindProperty("verticalScrollbarVisibility");
        _scrollSpacing = serializedObject.FindProperty("verticalScrollbarSpacing");
        _forwardEvents = serializedObject.FindProperty("forwardUnusedEventsToContainer");
        _onValueChanged = serializedObject.FindProperty("onValueChanged");
    }

    public override void OnInspectorGUI()
    {
        VerticalScrollRect script = (VerticalScrollRect)target;
        serializedObject.Update();
        EditorGUILayout.PropertyField(_content);
        EditorGUILayout.PropertyField(_scrollable);
        EditorGUILayout.PropertyField(_scrollingMovementType);
        if (script.ScrollingMovementType is BaseScrollRect.MovementType.Elastic)
        {
            EditorGUILayout.PropertyField(_elasticity);
        }
        EditorGUILayout.PropertyField(_inertia);
        EditorGUILayout.PropertyField(_decelerationRate);
        EditorGUILayout.PropertyField(_scrollSensitivity);
        EditorGUILayout.PropertyField(_viewport);
        EditorGUILayout.PropertyField(_scrollbar);
        if (script.VerticalScrollbar != null)
        {
            EditorGUILayout.PropertyField(_scrollbarVisibility);
            EditorGUILayout.PropertyField(_scrollSpacing);
        }
        EditorGUILayout.PropertyField(_forwardEvents);
        EditorGUILayout.PropertyField(_onValueChanged);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
