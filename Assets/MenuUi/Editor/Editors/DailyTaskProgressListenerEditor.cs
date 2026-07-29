using UnityEditor;
using Altzone.Scripts.Model.Poco.Game;


// Only show the relevant subcategory based on the selected category
[CustomEditor(typeof(DailyTaskProgressListener), true)]
public class DailyTaskProgressListenerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUI.EndDisabledGroup();

        SerializedProperty normalTask = serializedObject.FindProperty("_normalTaskType");
        SerializedProperty educationCategory = serializedObject.FindProperty("_educationCategoryType");

        SerializedProperty actionType = serializedObject.FindProperty("_educationCategoryActionType");
        SerializedProperty socialType = serializedObject.FindProperty("_educationCategorySocialType");
        SerializedProperty storyType = serializedObject.FindProperty("_educationCategoryStoryType");
        SerializedProperty cultureType = serializedObject.FindProperty("_educationCategoryCultureType");
        SerializedProperty ethicalType = serializedObject.FindProperty("_educationCategoryEthicalType");

        EditorGUILayout.PropertyField(normalTask);
        EditorGUILayout.PropertyField(educationCategory);

        EducationCategoryType category = (EducationCategoryType)educationCategory.enumValueIndex;


        if (category != EducationCategoryType.None)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Education task sub category", EditorStyles.boldLabel);
            switch (category)
            {
                case EducationCategoryType.Action:
                    EditorGUILayout.PropertyField(actionType);
                    break;
                case EducationCategoryType.Social:
                    EditorGUILayout.PropertyField(socialType);
                    break;
                case EducationCategoryType.Story:
                    EditorGUILayout.PropertyField(storyType);
                    break;
                case EducationCategoryType.Culture:
                    EditorGUILayout.PropertyField(cultureType);
                    break;
                case EducationCategoryType.Ethical:
                    EditorGUILayout.PropertyField(ethicalType);
                    break;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
