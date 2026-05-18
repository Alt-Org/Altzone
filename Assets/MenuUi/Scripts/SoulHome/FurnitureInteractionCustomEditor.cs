using System.Collections;
using System.Collections.Generic;
using MenuUI.Scripts.SoulHome;
using UnityEditor;
using UnityEngine;
using static UnityEditorInternal.VersionControl.ListControl;

[CustomEditor(typeof(FurnitureHandling))]
public class FurnitureInteractionCustomEditor : Editor
{
    static int width;
    static int height;

    private FurnitureHandling.Direction selectedDirection = FurnitureHandling.Direction.Front;

    private const int GridSize = 10;
    private const int CenterOffset = 3;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FurnitureHandling script = (FurnitureHandling)target;

        width = EditorGUILayout.IntField("Furniture width", width);
        height = EditorGUILayout.IntField("Furniture width", height);

        GUILayout.Space(15);
        EditorGUILayout.HelpBox("GREEN = Furniture Body (Size)\nYELLOW = Interaction Slots", MessageType.Info);

        GUILayout.Label("Custom Interaction Pattern", EditorStyles.boldLabel);

        selectedDirection = (FurnitureHandling.Direction)EditorGUILayout.EnumPopup("Editing State:", selectedDirection);

        Vector3Int size = new Vector3Int();

        bool isVertical = (selectedDirection == FurnitureHandling.Direction.Right || selectedDirection == FurnitureHandling.Direction.Left);
        if (!isVertical)
        {
            size.x = width;
            size.y = height;
        }
        else
        {
            size.y = width;
            size.x = height;
        }


        if (size.x <= 0 || size.y <= 0)
        {
            EditorGUILayout.HelpBox($"Size is 0.", MessageType.Warning);
            return;
        }

        int gridSize = 15;
        int offset = gridSize / 2;

        Vector2Int centeringShift = new Vector2Int(size.x / 2, -(size.y / 2));


        switch (selectedDirection)
        {
            case FurnitureHandling.Direction.Front:

                for (int y = -offset; y <= offset; y++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    for (int x = -offset; x <= offset; x++)
                    {

                        Vector2Int pos = new Vector2Int(x + centeringShift.x, y + centeringShift.y);

                        bool isInteraction = script.customInteractionOffsets.Contains(pos);

                        bool isFurnitureBody = (pos.x >= 0 && pos.x < size.x && pos.y <= 0 && pos.y > -size.y);

                        if (isFurnitureBody) GUI.backgroundColor = Color.green;
                        else if (isInteraction) GUI.backgroundColor = Color.yellow;
                        else GUI.backgroundColor = Color.white;

                        string buttonText = isFurnitureBody ? "O" : (isInteraction ? "X" : "");

                        if (GUILayout.Button(buttonText, GUILayout.Width(30), GUILayout.Height(30)))
                        {
                            if (!isFurnitureBody)
                            {
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
                break;

                //Doesnt change anything yet!!!
            case FurnitureHandling.Direction.Left:

                for (int y = -offset; y <= offset; y++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    for (int x = -offset; x <= offset; x++)
                    {

                        Vector2Int pos = new Vector2Int(x + centeringShift.x, y + centeringShift.y);

                        bool isInteraction = script.customInteractionOffsets.Contains(pos);

                        bool isFurnitureBody = (pos.x >= 0 && pos.x < size.x && pos.y <= 0 && pos.y > -size.y);

                        if (isFurnitureBody) GUI.backgroundColor = Color.green;
                        else if (isInteraction) GUI.backgroundColor = Color.yellow;
                        else GUI.backgroundColor = Color.white;

                        string buttonText = isFurnitureBody ? "O" : (isInteraction ? "X" : "");

                        if (GUILayout.Button(buttonText, GUILayout.Width(30), GUILayout.Height(30)))
                        {
                            if (!isFurnitureBody)
                            {
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
                break;
        }

        if (GUILayout.Button("Clear Pattern"))
        {
            script.customInteractionOffsets.Clear();
            EditorUtility.SetDirty(script);
        }
    }
}

