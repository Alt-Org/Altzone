using System;
using System.Collections.Generic;
using UnityEditor;
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

            foreach(AudioBlock block in _sfxList)
            {
                if(block.type == type) block.audioSource.Play();
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

            if (_sfxList.Count < value-1)
            {
                _sfxList[value-1].audioSource.Play();
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

        public void AddAudio(string name, AudioSourceType sourcetype, AudioTypeName type)
        {
            Transform parentTransform = transform;
            switch (sourcetype)
            {
                case AudioSourceType.Music:
                    break;
                case AudioSourceType.Sfx:
                    Transform sfxTransform = transform.Find("SoundFx");
                    if (sfxTransform == null)
                    {
                        GameObject gameObject = Instantiate(new GameObject(), transform);
                        gameObject.name = "SoundFx";
                        parentTransform = gameObject.transform;
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
                    break;
                default:
                    Transform undefined = transform.Find("Undefined");
                    if (undefined == null)
                    {
                        GameObject gameObject = Instantiate(new GameObject(), transform);
                        gameObject.name = "Undefined";
                        parentTransform = gameObject.transform;
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

    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        private AudioTypeName _sourceType = AudioTypeName.Music;
        private AudioSourceType _audioSourceType = AudioSourceType.Sfx;
        private string _newName = "";
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            AudioManager script = (AudioManager)target;
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("New AudioSource", EditorStyles.boldLabel);
            _audioSourceType = (AudioSourceType)EditorGUILayout.EnumPopup("Audio Source Type", _audioSourceType);
            switch (_audioSourceType)
            {
                case AudioSourceType.Music:
                    break;
                case AudioSourceType.Sfx:
                    _newName = EditorGUILayout.TextField("Name", _newName);
                    _sourceType = (AudioTypeName)EditorGUILayout.EnumPopup("Audio Type Name", _sourceType);
                    break;
                case AudioSourceType.Ambient:
                    _newName = EditorGUILayout.TextField("Name", _newName);
                    _sourceType = (AudioTypeName)EditorGUILayout.EnumPopup("Audio Type Name", _sourceType);
                    break;
            }
            if (GUILayout.Button("Add AudioSource"))
            {
                script.AddAudio(_newName, _audioSourceType, _sourceType);
                _newName = "";
            }
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
}
