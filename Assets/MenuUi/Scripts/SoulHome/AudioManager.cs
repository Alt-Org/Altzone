using System;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public enum AudioSourceType
    {
        Music,
        Sfx,
        Ambient
    }
    public enum AudioTypeName
    {
        None,
        Music,
        Click,
        PopupError,
        Revert,
        Save,
        Rotate,
        SetFurniture
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource _musicAudio;
        private MusicList _musicList;
        [SerializeField] private List<AudioBlock> _sfxList;
        [SerializeField] private List<AudioBlock> _ambientList;

        [SerializeField] private GameObject _audioSourcePrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            if (_musicAudio != null)
            {
                _musicList = _musicAudio.GetComponent<MusicList>();
            }
        }

        public void PlaySfxAudio(AudioTypeName type)
        {
            if (type == AudioTypeName.None) return;

            foreach (AudioBlock block in _sfxList)
            {
                if (block.type == type) block.audioSource.Play();
            }
        }

        public void PlaySfxAudio(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            foreach (AudioBlock block in _sfxList)
            {
                if (string.Equals(name, block.name)) block.audioSource.Play();
            }
        }
        public void PlaySfxAudio(int value)
        {
            if (value < 1) return;

            if (_sfxList.Count < value - 1)
            {
                _sfxList[value - 1].audioSource.Play();
            }
        }

        public void PlayAmbientAudio(AudioTypeName type)
        {
            if (type == AudioTypeName.None) return;

            foreach (AudioBlock block in _ambientList)
            {
                if (block.type == type) block.audioSource.Play();
            }
        }

        public void PlayAmbientAudio(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            foreach (AudioBlock block in _ambientList)
            {
                if (string.Equals(name, block.name)) block.audioSource.Play();
            }
        }
        public void PlayAmbientAudio(int value)
        {
            if (value < 1) return;

            if (_ambientList.Count < value - 1)
            {
                _ambientList[value - 1].audioSource.Play();
            }
        }

        public string PlayMusic()
        {
            if (_musicList == null) return null;
            return _musicList.PlayMusic();
        }

        public string NextMusicTrack()
        {
            if (_musicList == null) return null;
            return _musicList.NextTrack();
        }

        public string PrevMusicTrack()
        {
            if (_musicList == null) return null;
            return _musicList.PrevTrack();
        }

        public void StopMusic()
        {
            if (_musicList == null) return;
            _musicList.StopMusic();
        }

        [SerializeField]  public List<string> _audioTypes = new();

        List<string> AudioTypesInHierarcy
        {
            get
            {
                List<string> AudioTypes = new();
                foreach (Transform transform in transform)
                {
                    foreach (Transform transform2 in transform)
                    {
                        if(transform2.GetComponent<AudioSource>() == null)
                        {
                            AudioTypes.Add(transform2.gameObject.name);
                        }
                    }
                }
                return AudioTypes;
            }
        }

        public void AddAudio(string name, AudioSourceType sourcetype, AudioTypeName type)
        {
            Transform parentTransform = transform;
            switch (sourcetype)
            {
                case AudioSourceType.Music:
                    Transform musicTransform = transform.Find("Music");
                    if (musicTransform == null || musicTransform.GetComponent<MusicList>() == null)
                    {
                        GameObject gameObject = Instantiate(new GameObject(), transform);
                        gameObject.name = "Music";
                    }
                    if(musicTransform.GetComponent<AudioSource>() == null) gameObject.AddComponent<AudioSource>();
                    if(musicTransform.GetComponent<MusicList>() == null) gameObject.AddComponent<MusicList>();
                    if(musicTransform.GetComponent<SetVolume>() == null) gameObject.AddComponent<SetVolume>();
                    return;
                case AudioSourceType.Sfx:
                    Transform sfxTransform = transform.Find("SoundFx");
                    if (sfxTransform == null)
                    {
                        GameObject gameObject = Instantiate(new GameObject(), transform);
                        gameObject.name = "SoundFx";
                        parentTransform = gameObject.transform;
                    }
                    else
                    {
                        parentTransform = sfxTransform;
                    }
                    break;
                case AudioSourceType.Ambient:
                    Transform ambient = transform.Find("Ambient");
                    if (ambient == null)
                    {
                        GameObject gameObject = Instantiate(new GameObject(), transform);
                        gameObject.name = "Ambient";
                        parentTransform = gameObject.transform;
                    }
                    else
                    {
                        parentTransform = ambient;
                    }
                    break;
                default:
                    Transform undefined = transform.Find("Undefined");
                    if (undefined == null)
                    {
                        GameObject gameObject = Instantiate(new GameObject(), transform);
                        gameObject.name = "Undefined";
                        parentTransform = gameObject.transform;
                    }
                    else
                    {
                        parentTransform = undefined;
                    }
                    break;
            }
            GameObject gameObject2 = Instantiate(_audioSourcePrefab, parentTransform);
            if (!string.IsNullOrWhiteSpace(name))
                gameObject2.name = name;
            AudioBlock audioBlock = new(gameObject2.name);
            audioBlock.type = type;
            audioBlock.audioSource = gameObject2.GetComponent<AudioSource>();
            if(sourcetype is AudioSourceType.Sfx)
                _sfxList.Add(audioBlock);
            else if(sourcetype is AudioSourceType.Ambient)
                _ambientList.Add(audioBlock);
        }
    }

    [Serializable]
    public class AudioBlock
    {
        public string name;
        public AudioSource audioSource;
        public AudioTypeName type;

        public AudioBlock(string name)
        {
            this.name = name;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        private AudioTypeName _sourceType = AudioTypeName.Music;
        private AudioSourceType _audioSourceType = AudioSourceType.Sfx;
        private string _newName = "";
        private string _newTypeName = "";
        [SerializeField] List<string> _audioTypes = new();
        int index = 0;
        int index2 = 0;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            var prop = serializedObject.FindProperty("_audioTypes");
            _audioTypes.Clear();
            int i = 0;
            for (i = 0; i < prop.arraySize; i++)
            {
                _audioTypes.Add(prop.GetArrayElementAtIndex(i).stringValue);
            }

            AudioManager script = (AudioManager)target;

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("AudioTypes", EditorStyles.boldLabel);
            index= EditorGUILayout.Popup("AudioTypes", index, _audioTypes.ToArray());
            _newTypeName = EditorGUILayout.TextField("Name", _newTypeName);
            if (GUILayout.Button("Add AudioType"))
            {
                if (!string.IsNullOrWhiteSpace(_newTypeName))
                {
                    if (!_audioTypes.Contains(_newTypeName.Trim())) { 
                        _audioTypes.Add(_newTypeName.Trim());
                        prop.InsertArrayElementAtIndex(i);
                        prop.GetArrayElementAtIndex(i).stringValue = _newTypeName.Trim();
                    }
                }
                _newTypeName = "";
            }
            if (GUILayout.Button("Remove AudioType"))
            {
                _audioTypes.Remove(_audioTypes[index]);
                prop.DeleteArrayElementAtIndex(index);
                if (index != 0) index--;
                _newTypeName = "";
            }

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("New AudioSource", EditorStyles.boldLabel);
            _audioSourceType = (AudioSourceType)EditorGUILayout.EnumPopup("Audio Source Type", _audioSourceType);
            switch (_audioSourceType)
            {
                case AudioSourceType.Music:
                    break;
                case AudioSourceType.Sfx:
                    _newName = EditorGUILayout.TextField("Name", _newName);
                    index2 = EditorGUILayout.Popup("AudioTypes", index2, _audioTypes.ToArray());
                    break;
                case AudioSourceType.Ambient:
                    _newName = EditorGUILayout.TextField("Name", _newName);
                    index2 = EditorGUILayout.Popup("AudioTypes", index2, _audioTypes.ToArray());
                    break;
            }
            if (GUILayout.Button("Add AudioSource"))
            {
                script.AddAudio(_newName, _audioSourceType, _sourceType);
                _newName = "";
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif
}
