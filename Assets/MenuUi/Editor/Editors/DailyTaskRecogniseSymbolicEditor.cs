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

        SerializedProperty questionOption1 = serializedObject.FindProperty("_questionOption1");
        SerializedProperty questionOption2 = serializedObject.FindProperty("_questionOption2");
        SerializedProperty questionOption3 = serializedObject.FindProperty("_questionOption3");
        SerializedProperty answer = serializedObject.FindProperty("_answer");

        educationCategory.enumValueIndex = (int)EducationCategoryType.Story;
        actionType.enumValueIndex = (int)TaskEducationStoryType.Press3SymbolFurniture;

        EditorGUILayout.PropertyField(questionOption1);
        EditorGUILayout.PropertyField(questionOption2);
        EditorGUILayout.PropertyField(questionOption3);
        EditorGUILayout.PropertyField(answer);

        serializedObject.ApplyModifiedProperties();
    }
}
