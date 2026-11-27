using Altzone.Scripts.Model.Poco.Game;
using MenuUI.Scripts.SoulHome;
using UnityEditor;
using UnityEngine;
using static MenuUI.Scripts.SoulHome.FurnitureHandling;

[CustomEditor(typeof(FurnitureHandling))]
public class FurnitureHandlingEditor : Editor
{
    protected FurnitureHandling _script;

    private SerializedProperty _name;
    private SerializedProperty _furnitureSpriteFront;
    private SerializedProperty _furnitureSpriteRight;
    private SerializedProperty _furnitureSpriteLeft;
    private SerializedProperty _furnitureSpriteBack;
    private SerializedProperty _defaultSpriteDirection;

    private SerializedProperty _spriteCanBeFlipped;
    private SerializedProperty _sortingGroup;
    private SerializedProperty _trayFurnitureObject;
    private SerializedProperty cullCheckAmount;

    private SerializedProperty _usesAnimation;

    private SerializedProperty _interactionStyle;
    private SerializedProperty _pattern;

    private SerializedProperty widthProp;
    private SerializedProperty heightProp;

    private void OnEnable()
    {
        _script = target as FurnitureHandling;
        _name = serializedObject.FindProperty("_name");
        _furnitureSpriteFront = serializedObject.FindProperty("_furnitureSpriteFront");
        _furnitureSpriteRight = serializedObject.FindProperty("_furnitureSpriteRight");
        _furnitureSpriteLeft = serializedObject.FindProperty("_furnitureSpriteLeft");
        _furnitureSpriteBack = serializedObject.FindProperty("_furnitureSpriteBack");
        _defaultSpriteDirection = serializedObject.FindProperty("_defaultSpriteDirection");
        _spriteCanBeFlipped = serializedObject.FindProperty("_spriteCanBeFlipped");
        _sortingGroup = serializedObject.FindProperty("_sortingGroup");
        _trayFurnitureObject = serializedObject.FindProperty("_trayFurnitureObject");
        cullCheckAmount = serializedObject.FindProperty("cullCheckAmount");
        _usesAnimation = serializedObject.FindProperty("_usesAnimation");
        _interactionStyle = serializedObject.FindProperty("_interactionStyle");
        _pattern = serializedObject.FindProperty("_pattern");
        widthProp = serializedObject.FindProperty("_width");
        heightProp = serializedObject.FindProperty("_height");

    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawBaseEditor();

        

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBaseEditor()
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_name);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_furnitureSpriteFront);
        EditorGUILayout.PropertyField(_furnitureSpriteLeft);
        EditorGUILayout.PropertyField(_furnitureSpriteRight);
        EditorGUILayout.PropertyField(_furnitureSpriteBack);
        EditorGUILayout.PropertyField(_defaultSpriteDirection);
        EditorGUILayout.PropertyField(_spriteCanBeFlipped);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_sortingGroup);
        EditorGUILayout.PropertyField(_trayFurnitureObject);
        EditorGUILayout.PropertyField(cullCheckAmount);
        EditorGUILayout.Space();

        DrawInteractSection();
    }

    private void DrawInteractSection()
    {
        if (_script.Furniture.Place is not (FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)) 
        {
            EditorGUILayout.HelpBox($"Only Floor Furniture can have interactions.", MessageType.Info);
            return;
        }
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_usesAnimation);
        if (!_usesAnimation.boolValue) return;
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_interactionStyle);
        if(_interactionStyle.enumValueIndex != (int)AnimationTriggerStyle.AvatarInteract) return;
        EditorGUILayout.PropertyField(_pattern);
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(widthProp);
        EditorGUILayout.PropertyField(heightProp);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Space();

        widthProp.intValue = _script.Furniture.FurnitureSize.x;
        heightProp.intValue = _script.Furniture.FurnitureSize.z;

        GUILayout.Space(15);
        EditorGUILayout.HelpBox("GREEN = Furniture Body (Size)\nYELLOW = Interaction Slots", MessageType.Info);
        GUILayout.Label("Custom Interaction Pattern (Front View)", EditorStyles.boldLabel);

        int width = _script.Furniture.FurnitureSize.x;
        int height = _script.Furniture.FurnitureSize.z;

        if (width <= 0 || height <= 0)
        {
            EditorGUILayout.HelpBox($"Size is 0 or negative.", MessageType.Warning);
            return;
        }

        for (int y = 1; y >= -height; y--)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int x = -1; x <= width; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                bool isInteraction = _script.customInteractionOffsets.Contains(pos);
                bool isFurnitureBody = (x >= 0 && x < width && y <= 0 && y > -height);

                if (isFurnitureBody) GUI.backgroundColor = Color.green;
                else if (isInteraction) GUI.backgroundColor = Color.yellow;
                else GUI.backgroundColor = Color.white;

                string buttonText = isFurnitureBody ? "F" : (isInteraction ? "I" : "");

                if (GUILayout.Button(buttonText, GUILayout.Width(30), GUILayout.Height(30)))
                {
                    if (!isFurnitureBody)
                    {
                        Undo.RecordObject(_script, "Modify Custom Interaction Slot");
                        if (isInteraction) _script.customInteractionOffsets.Remove(pos);
                        else _script.customInteractionOffsets.Add(pos);

                        EditorUtility.SetDirty(_script);
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Clear Pattern"))
        {
            Undo.RecordObject(_script, "Clear Interaction Pattern");
            _script.customInteractionOffsets.Clear();
            EditorUtility.SetDirty(_script);
        }
    }
}
