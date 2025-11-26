using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class DailyTaskData
{
    public string title;
    public string englishTitle;
    public string description;
    public string englishDescription;
    public int amount;
    public int coins;
    public int points;
}

[System.Serializable]
public class NormalDailyTaskData : DailyTaskData
{
    public string type;
}

[System.Serializable]
public class EducationDailyTaskData : DailyTaskData
{
    public string educationCategoryType;
    public string educationCategoryTaskType;
}

//[CreateAssetMenu(fileName = "DailyTaskConfig")]
public class DailyTaskConfig : ScriptableObject
{
    private static DailyTaskConfig _instance;

    public static DailyTaskConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<DailyTaskConfig>(nameof(DailyTaskConfig));
            }

            return _instance;
        }
    }

    [SerializeField] private List<NormalDailyTaskData> _normalDailyTasks;
    [SerializeField] private List<EducationDailyTaskData> _educationDailyTasks;

    public List<NormalDailyTaskData> GetNormalTasks() => _normalDailyTasks;
    public List<EducationDailyTaskData> GetEducationTasks() => _educationDailyTasks;
}

#region Editor script
#if UNITY_EDITOR
[CustomEditor(typeof(DailyTaskConfig))]
public class DailyTaskConfigEditor : Editor
{
    private SerializedProperty normalTasksProp;
    private SerializedProperty educationTasksProp;

    private readonly string[] categories = { "action", "social", "story", "culture", "ethical" };

    private Dictionary<string, ReorderableList> categoryLists;
    private Dictionary<string, List<int>> categoryIndices;

    private GUIStyle foldoutBoldStyle;

    private string CategoryKey(string cat) => $"DailyTaskConfigEditor:{target.GetInstanceID()}:{cat}";

    private void OnEnable()
    {
        normalTasksProp = serializedObject.FindProperty("_normalDailyTasks");
        educationTasksProp = serializedObject.FindProperty("_educationDailyTasks");

        categoryLists = new Dictionary<string, ReorderableList>();
        categoryIndices = new Dictionary<string, List<int>>();

        foldoutBoldStyle = new GUIStyle(EditorStyles.foldout);
        foldoutBoldStyle.fontStyle = FontStyle.Bold;

        foreach (var cat in categories)
        {
            RefreshCategoryIndices(cat);

            var list = new ReorderableList(categoryIndices[cat], typeof(int), true, false, true, true);

            // Draw elements
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= categoryIndices[cat].Count) return;
                int realIndex = categoryIndices[cat][index];
                var element = educationTasksProp.GetArrayElementAtIndex(realIndex);

                Rect paddedRect = new(rect.x + 8, rect.y + 1, rect.width - 8, rect.height);

                string taskTitle = element.FindPropertyRelative("title").stringValue;
                GUIContent content = new(string.IsNullOrEmpty(taskTitle) ? "New Task" : taskTitle);

                EditorGUI.PropertyField(paddedRect, element, content, true);
            };
            
            // Element height
            list.elementHeightCallback = (int index) =>
            {
                if (index >= categoryIndices[cat].Count) return 0;
                int realIndex = categoryIndices[cat][index];
                var element = educationTasksProp.GetArrayElementAtIndex(realIndex);
                return EditorGUI.GetPropertyHeight(element, true);
            };

            // Add new element to list
            list.onAddCallback = (ReorderableList l) =>
            {
                int newIndex = educationTasksProp.arraySize;
                educationTasksProp.InsertArrayElementAtIndex(newIndex);
                var newElement = educationTasksProp.GetArrayElementAtIndex(newIndex);
                newElement.FindPropertyRelative("educationCategoryType").stringValue = cat;
                newElement.FindPropertyRelative("title").stringValue = "New Task";

                RefreshCategoryIndices(cat);
                l.list = categoryIndices[cat];
                Repaint();
            };

            // Remove element from list
            list.onRemoveCallback = (ReorderableList l) =>
            {
                int index = l.index;
                if (index < 0 || index >= categoryIndices[cat].Count) return;

                int realIndex = categoryIndices[cat][index];
                educationTasksProp.DeleteArrayElementAtIndex(realIndex);

                RefreshCategoryIndices(cat);
                l.list = categoryIndices[cat];
                Repaint();
            };

            categoryLists[cat] = list;
        }
    }

    private void RefreshCategoryIndices(string category)
    {
        var indices = new List<int>();
        for (int i = 0; i < educationTasksProp.arraySize; i++)
        {
            var element = educationTasksProp.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("educationCategoryType").stringValue == category)
                indices.Add(i);
        }

        categoryIndices[category] = indices;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(normalTasksProp, new GUIContent("Normal Daily Tasks"), true);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Education Daily Tasks", EditorStyles.boldLabel);

        foreach (var cat in categories)
        {
            EditorGUILayout.BeginHorizontal();

            // Foldout arrow and label
            bool open = SessionState.GetBool(CategoryKey(cat), false);
            bool newOpen = EditorGUILayout.Foldout(open, char.ToUpper(cat[0]) + cat.Substring(1), true, foldoutBoldStyle);
            if (newOpen != open) SessionState.SetBool(CategoryKey(cat), newOpen);

            // Count box on the right
            int count = 0;
            for (int i = 0; i < educationTasksProp.arraySize; i++)
            {
                var element = educationTasksProp.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("educationCategoryType").stringValue == cat)
                    count++;
            }
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField(count, GUILayout.Width(48));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (newOpen)
            {
                EditorGUI.indentLevel++;
                categoryLists[cat].DoLayoutList();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion
