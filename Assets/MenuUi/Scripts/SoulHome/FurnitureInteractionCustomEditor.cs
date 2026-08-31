using MenuUI.Scripts.SoulHome;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FurnitureHandling))]
public class FurnitureInteractionCustomEditor : Editor
{
    private SerializedProperty widthProp;
    private SerializedProperty heightProp;

    private void OnEnable()
    {
        widthProp = serializedObject.FindProperty("_width");
        heightProp = serializedObject.FindProperty("_height");
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();

        FurnitureHandling script = (FurnitureHandling)target;

        if (widthProp.intValue < 1) widthProp.intValue = 1;
        if (heightProp.intValue < 1) heightProp.intValue = 1;

        GUILayout.Space(15);
        EditorGUILayout.HelpBox("GREEN = Furniture Body (Size)\nYELLOW = Interaction Slots", MessageType.Info);
        GUILayout.Label("Custom Interaction Pattern (Front View)", EditorStyles.boldLabel);

        int width = widthProp.intValue;
        int height = heightProp.intValue;

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

                bool isInteraction = script.customInteractionOffsets.Contains(pos);
                bool isFurnitureBody = (x >= 0 && x < width && y <= 0 && y > -height);

                if (isFurnitureBody) GUI.backgroundColor = Color.green;
                else if (isInteraction) GUI.backgroundColor = Color.yellow;
                else GUI.backgroundColor = Color.white;

                string buttonText = isFurnitureBody ? "F" : (isInteraction ? "I" : "");

                if (GUILayout.Button(buttonText, GUILayout.Width(30), GUILayout.Height(30)))
                {
                    if (!isFurnitureBody)
                    {
                        Undo.RecordObject(script, "Modify Custom Interaction Slot");
                        if (isInteraction) script.customInteractionOffsets.Remove(pos);
                        else script.customInteractionOffsets.Add(pos);

                        EditorUtility.SetDirty(script);
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Clear Pattern"))
        {
            Undo.RecordObject(script, "Clear Interaction Pattern");
            script.customInteractionOffsets.Clear();
            EditorUtility.SetDirty(script);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
