using MenuUI.Scripts.SoulHome;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FurnitureHandling))]
public class FurnitureInteractionCustomEditor : Editor
{
    protected FurnitureHandling _script;
    private SerializedProperty widthProp;
    private SerializedProperty heightProp;

    private void OnEnable()
    {
        _script = target as FurnitureHandling;
        widthProp = serializedObject.FindProperty("_width");
        heightProp = serializedObject.FindProperty("_height");
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();

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

        serializedObject.ApplyModifiedProperties();
    }
}
