using UnityEditor;
using Altzone.Scripts.Model.Poco.Game;


// Only show the relevant subcategory based on the selected category
[CustomEditor(typeof(DailyTaskRecogniseSymbolic), true)]
public class DailyTaskRecogniseSymbolicEditor : DailyTaskProgressListenerEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty educationCategory = serializedObject.FindProperty("_educationCategoryType");
        SerializedProperty actionType = serializedObject.FindProperty("_educationCategoryStoryType");

        SerializedProperty questionOption1Fi = serializedObject.FindProperty("_questionOption1Fi");
        SerializedProperty questionOption1En = serializedObject.FindProperty("_questionOption1En");
        SerializedProperty questionOption2Fi = serializedObject.FindProperty("_questionOption2Fi");
        SerializedProperty questionOption2En = serializedObject.FindProperty("_questionOption2En");
        SerializedProperty questionOption3Fi = serializedObject.FindProperty("_questionOption3Fi");
        SerializedProperty questionOption3En = serializedObject.FindProperty("_questionOption3En");
        SerializedProperty answer = serializedObject.FindProperty("_answer");

        educationCategory.enumValueIndex = (int)EducationCategoryType.Story;
        actionType.enumValueIndex = (int)TaskEducationStoryType.Press3SymbolFurniture;

        EditorGUILayout.PropertyField(questionOption1Fi);
        EditorGUILayout.PropertyField(questionOption1En);
        EditorGUILayout.PropertyField(questionOption2Fi);
        EditorGUILayout.PropertyField(questionOption2En);
        EditorGUILayout.PropertyField(questionOption3Fi);
        EditorGUILayout.PropertyField(questionOption3En);
        EditorGUILayout.PropertyField(answer);

        serializedObject.ApplyModifiedProperties();
    }
}
