using UnityEditor;
using Altzone.Scripts.Model.Poco.Game;


// Only show the relevant subcategory based on the selected category
[CustomEditor(typeof(DailyTaskProgressListenerMakeMusic), true)]
public class DailyTaskProgressListenerMakeMusicEditor : DailyTaskProgressListenerEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        SerializedProperty buttonsToClick = serializedObject.FindProperty("_buttonsToClick");
        EditorGUILayout.PropertyField(buttonsToClick);

        SerializedProperty naviButtonsToDisable = serializedObject.FindProperty("_naviButtonsToDisable");
        EditorGUILayout.PropertyField(naviButtonsToDisable);

        serializedObject.ApplyModifiedProperties();
    }
}
