using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalScrollRect : BaseScrollRect
{
    public override bool VerticallyScrollable
    {
        get { return false; }
    }

    public override Scrollbar VerticalScrollbar
    {
        get { return null; }
    }

    public override ScrollbarVisibility VerticalScrollbarVisibilityMode
    {
        get { return HorizontalScrollbarVisibilityMode; }
    }

    public override float VerticalScrollbarSpacing
    {
        get { return 0f; }
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Horizontal Scroll Rect", false, 10)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/HorizontalScrollView"));
        go.name = "HorizontalScrollView";
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(HorizontalScrollRect), true)]
[CanEditMultipleObjects]
/// <summary>
/// Custom Editor for the Toggle Component.
/// Extend this class to write a custom editor for a component derived from Toggle.
/// </summary>
public class HorizontalScrollRectEditor : Editor
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
        _scrollable = serializedObject.FindProperty("horizontallyScrollable");
        _scrollingMovementType = serializedObject.FindProperty("scrollingMovementType");
        _elasticity = serializedObject.FindProperty("elasticity");
        _inertia = serializedObject.FindProperty("inertia");
        _decelerationRate = serializedObject.FindProperty("decelerationRate");
        _scrollSensitivity = serializedObject.FindProperty("scrollSensitivity");
        _viewport = serializedObject.FindProperty("viewportRectTransform");
        _scrollbar = serializedObject.FindProperty("horizontalScrollbar");
        _scrollbarVisibility = serializedObject.FindProperty("horizontalScrollbarVisibility");
        _scrollSpacing = serializedObject.FindProperty("horizontalScrollbarSpacing");
        _forwardEvents = serializedObject.FindProperty("forwardUnusedEventsToContainer");
        _onValueChanged = serializedObject.FindProperty("onValueChanged");
    }

    public override void OnInspectorGUI()
    {
        HorizontalScrollRect script = (HorizontalScrollRect)target;
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
        if (script.HorizontalScrollbar != null)
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
