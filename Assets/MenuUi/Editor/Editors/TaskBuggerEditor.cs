using UnityEditor;
using Altzone.Scripts.Model.Poco.Game;


// Only show the relevant subcategory based on the selected category
[CustomEditor(typeof(TaskBugger), true)]
public class TaskBuggerEditor : DailyTaskProgressListenerEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        SerializedProperty buggedText = serializedObject.FindProperty("_buggedText");
        SerializedProperty buggedTextString = serializedObject.FindProperty("_buggedTextString");
        SerializedProperty wheelPrefab = serializedObject.FindProperty("_wheelPrefab");

        EditorGUILayout.PropertyField(buggedText);
        EditorGUILayout.PropertyField(buggedTextString);
        EditorGUILayout.PropertyField(wheelPrefab);

        serializedObject.ApplyModifiedProperties();
    }
}
