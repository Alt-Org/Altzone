using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DailyTaskProgressListener : MonoBehaviour
{
    [Header("Normal task")]
    [SerializeField] protected TaskNormalType _normalTaskType = TaskNormalType.Undefined;

    [Header("Education task main category")]
    [SerializeField] protected EducationCategoryType _educationCategoryType = EducationCategoryType.None;

    // [Header("Education task sub categories\n (Only one of these will be used!)")]
    [SerializeField] protected TaskEducationActionType _educationCategoryActionType = TaskEducationActionType.BlowUpYourCharacter;
    [SerializeField] protected TaskEducationSocialType _educationCategorySocialType = TaskEducationSocialType.AddNewFriend;
    [SerializeField] protected TaskEducationStoryType _educationCategoryStoryType = TaskEducationStoryType.ClickCharacterDescription;
    [SerializeField] protected TaskEducationCultureType _educationCategoryCultureType = TaskEducationCultureType.ClickKnownArtIdeaPerson;
    [SerializeField] protected TaskEducationEthicalType _educationCategoryEthicalType = TaskEducationEthicalType.ClickBuyable;

    private bool _on = false;
    [HideInInspector] public bool On { get => _on; }

    protected virtual void Start()
    {
        try
        {
            DailyTaskProgressManager.OnTaskChange += SetState;
            PlayerTask task = DailyTaskProgressManager.Instance.CurrentPlayerTask;
            if (task == null)
            {
                _on = false;
                return;
            }

            if (_normalTaskType != TaskNormalType.Undefined)
            {
                _on = (_normalTaskType == task.Type);
                return;
            }

            if (_educationCategoryType != EducationCategoryType.None)
            {
                switch (task.EducationCategory)
                {
                    case EducationCategoryType.Action: _on = (_educationCategoryActionType == task.EducationActionType); break;
                    case EducationCategoryType.Social: _on = (_educationCategorySocialType == task.EducationSocialType); break;
                    case EducationCategoryType.Story: _on = (_educationCategoryStoryType == task.EducationStoryType); break;
                    case EducationCategoryType.Culture: _on = (_educationCategoryCultureType == task.EducationCultureType); break;
                    case EducationCategoryType.Ethical: _on = (_educationCategoryEthicalType == task.EducationEthicalType); break;
                    default: _on = false; break;
                }
            }
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    private void OnDestroy()
    {
        try
        {
            DailyTaskProgressManager.OnTaskChange -= SetState;
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    /// <summary>
    /// Call this function from location where it's corresponding<br/>
    /// task will be seen as valid daily task progress.<br/><br/>
    /// Normal use case: give an integer value of 1 or greater as a string.<br/><br/>
    /// Special use case: give a character name or other unique identifier as a string<br/>
    /// (eg. Start 3 battles with different characters).
    /// </summary>
    public void UpdateProgress(string value)
    {
        try
        {
            if (!_on)
                return;
            
            if (_normalTaskType != TaskNormalType.Undefined)
            {
                DailyTaskProgressManager.Instance.UpdateTaskProgress(_normalTaskType, value);
                return;
            }

            switch (_educationCategoryType)
            {
                case EducationCategoryType.Action:
                    {
                        DailyTaskProgressManager.Instance.UpdateTaskProgress(_educationCategoryActionType, value);
                        break;
                    }
                case EducationCategoryType.Social:
                    {
                        DailyTaskProgressManager.Instance.UpdateTaskProgress(_educationCategorySocialType, value);
                        break;
                    }
                case EducationCategoryType.Story:
                    {
                        DailyTaskProgressManager.Instance.UpdateTaskProgress(_educationCategoryStoryType, value);
                        break;
                    }
                case EducationCategoryType.Culture:
                    {
                        DailyTaskProgressManager.Instance.UpdateTaskProgress(_educationCategoryCultureType, value);
                        break;
                    }
                case EducationCategoryType.Ethical:
                    {
                        DailyTaskProgressManager.Instance.UpdateTaskProgress(_educationCategoryEthicalType, value);
                        break;
                    }
                default: break;
            }

            return;
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    public virtual void SetState(PlayerTask task)
    {
        //-----TEST CODE-----
        if (_normalTaskType == TaskNormalType.Test)
        {
            _on = true;
            return;
        }
        //-------------------

        if (task == null)
        {
            _on = false;
            return;
        }

        if (_normalTaskType != TaskNormalType.Undefined)
        {
            _on = (_normalTaskType == task.Type);
            return;
        }

        if (_educationCategoryType != EducationCategoryType.None)
        {
            switch (task.EducationCategory)
            {
                case EducationCategoryType.Action: _on = (_educationCategoryActionType == task.EducationActionType); break;
                case EducationCategoryType.Social: _on = (_educationCategorySocialType == task.EducationSocialType); break;
                case EducationCategoryType.Story: _on = (_educationCategoryStoryType == task.EducationStoryType); break;
                case EducationCategoryType.Culture: _on = (_educationCategoryCultureType == task.EducationCultureType); break;
                case EducationCategoryType.Ethical: _on = (_educationCategoryEthicalType == task.EducationEthicalType); break;
                default: _on = false; break;
            }
        }
    }
}

#if UNITY_EDITOR
// Only show the relevant subcategory based on the selected category
[CustomEditor(typeof(DailyTaskProgressListener))]
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
#endif
