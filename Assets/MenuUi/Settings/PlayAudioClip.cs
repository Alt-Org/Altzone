using MenuUI.Scripts.SoulHome;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AudioTypeName = MenuUI.Scripts.SoulHome.AudioTypeName;

enum AudioSelection
{
    Type,
    Name,
    ID
}

public class PlayAudioClip : MonoBehaviour
{
    [SerializeField]
    private AudioSelection _audioSelection = AudioSelection.Type;
    [SerializeField]
    AudioTypeName _audioType = AudioTypeName.None;
    [SerializeField]
    string _audioName = "";
    [SerializeField]
    int _audioId = 0;
    [SerializeField, Tooltip("If enabled the script will add listener to the button component's onClick event to play the audio clip.")]
    bool _useOnClickEvent = true;


    private void Start()
    {
        if (_useOnClickEvent)
        {
            GetComponent<Button>()?.onClick.AddListener(PlayAudio);
        }
        GetComponent<Toggle>()?.onValueChanged.AddListener(PlayAudio);
    }

    //Since we use editor calls we omit this function on build time
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void Reset()
    {
        Button source = GetComponent<Button>();
        Toggle light = GetComponent<Toggle>();
#if UNITY_EDITOR
        if (source == null && light == null)
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Choose a Component", "You are missing one of the required componets. Please choose one to add", "Button", "Toggle"))
            {
                gameObject.AddComponent<Button>();
            }
            else
            {
                gameObject.AddComponent<Toggle>();
            }
        }
#endif
    }

    public void PlayAudio(bool value) => PlayAudio();
    public void PlayAudio()
    {
        AudioManager manager = AudioManager.Instance;
        if (manager == null)
        {
            Debug.LogError("Cannot find audio manager. Check if AudioManager is added to the scene.");
            return;
        }
        if (_audioSelection == AudioSelection.Type)
            manager.PlaySfxAudio(_audioType);
        else if(_audioSelection == AudioSelection.Name)
            manager.PlaySfxAudio(_audioName);
        else if (_audioSelection == AudioSelection.ID)
            manager.PlaySfxAudio(_audioId);
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(PlayAudioClip))]
    public class PlayAudioClipEditor : Editor
    {
        SerializedProperty section;
        SerializedProperty sectionA;
        SerializedProperty sectionB;
        SerializedProperty sectionC;
        SerializedProperty sectionOnClickBool;

        void OnEnable()
        {
            section = serializedObject.FindProperty(nameof(_audioSelection));
            sectionA = serializedObject.FindProperty(nameof(_audioType));
            sectionB = serializedObject.FindProperty(nameof(_audioName));
            sectionC = serializedObject.FindProperty(nameof(_audioId));
            sectionOnClickBool = serializedObject.FindProperty(nameof(_useOnClickEvent));
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(section);
            switch ((AudioSelection)section.enumValueIndex)
            {
                case AudioSelection.Type:
                    EditorGUILayout.PropertyField(sectionA);
                    break;
                case AudioSelection.Name:
                    EditorGUILayout.PropertyField(sectionB);
                    break;
                case AudioSelection.ID:
                    EditorGUILayout.PropertyField(sectionC);
                    break;
            }
            EditorGUILayout.PropertyField(sectionOnClickBool);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}
