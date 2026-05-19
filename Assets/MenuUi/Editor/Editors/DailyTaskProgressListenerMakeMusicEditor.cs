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

        SerializedProperty buttons = serializedObject.FindProperty("_buttons");
        EditorGUILayout.PropertyField(buttons);

        serializedObject.ApplyModifiedProperties();
    }
}
