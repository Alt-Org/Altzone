using UnityEditor;
using Altzone.Scripts.Model.Poco.Game;


// Only show the relevant subcategory based on the selected category
[CustomEditor(typeof(TaskBugger), true)]
public class TaskBuggerEditor : DailyTaskProgressListenerEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty educationCategory = serializedObject.FindProperty("_educationCategoryType");
        SerializedProperty actionType = serializedObject.FindProperty("_educationCategoryActionType");

        SerializedProperty buggedText = serializedObject.FindProperty("_buggedText");
        SerializedProperty buggedTextString = serializedObject.FindProperty("_buggedTextString");
        SerializedProperty wheelPrefab = serializedObject.FindProperty("_wheelPrefab");

        educationCategory.enumValueIndex = (int)EducationCategoryType.Action;
        actionType.enumValueIndex = (int)TaskEducationActionType.FindBug;

        EditorGUILayout.PropertyField(buggedText);
        EditorGUILayout.PropertyField(buggedTextString);
        EditorGUILayout.PropertyField(wheelPrefab);

        serializedObject.ApplyModifiedProperties();
    }
}
