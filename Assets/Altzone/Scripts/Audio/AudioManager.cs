using System;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;

namespace Altzone.Scripts.Audio
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
        private MusicHandler _musicHandler;
        [SerializeField] private JukeboxController _jukebox;
        [SerializeField] private List<AudioBlock> _sfxList;
        [SerializeField] private List<AudioBlock> _ambientList;

        [SerializeField] private GameObject _audioSourcePrefab;
        [SerializeField] public List<string> _audioSections = new();

        public JukeboxSong[] JukeBoxSongs => _jukebox.Songs;
        public Queue<JukeboxSong> JukeBoxQueue => _jukebox.SongQueue;
        public JukeboxSong JukeBoxCurrentSong => _jukebox.CurrentSong;
        public JukeboxController Jukebox { get => _jukebox; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
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
                _musicHandler = _musicAudio.GetComponent<MusicHandler>();
            }
        }

        public void PlaySfxAudioWithType(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return;

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

        public void PlayAmbientAudioWithType(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return;

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

        public string PlayMusic(MusicSection section, int musicindex = -1)
        {
            if (_musicHandler == null) return null;
            else if (section == MusicSection.SoulHome)
            {
                if (JukeBoxCurrentSong?.songs != null)
                {
                    _jukebox.ContinueSong();
                    return JukeBoxCurrentSong.songName;
                }
            }
            return _musicHandler.PlayMusic(section, musicindex);
        }

        public string NextMusicTrack()
        {
            if (_musicHandler == null) return null;
            return _musicHandler.NextTrack();
        }

        public string PrevMusicTrack()
        {
            if (_musicHandler == null) return null;
            return _musicHandler.PrevTrack();
        }

        public void StopMusic()
        {
            if (_musicHandler == null) return;
            if (JukeBoxCurrentSong?.songs != null)
            {
                _jukebox.StopSong();
            }
            _musicHandler.StopMusic();
        }

        #region Internal Editor Methods
        List<string> AudioTypesInHierarcy
        {
            get
            {
                List<string> AudioTypes = new();
                foreach (Transform transform in transform)
                {
                    foreach (Transform transform2 in transform)
                    {
                        if (transform2.GetComponent<AudioSource>() == null)
                        {
                            AudioTypes.Add(transform2.gameObject.name);
                        }
                    }
                }
                return AudioTypes;
            }
        }

        public void AddAudio(string name, AudioSourceType sourcetype, string section, string type)
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
                    if (musicTransform.GetComponent<AudioSource>() == null) gameObject.AddComponent<AudioSource>();
                    if (musicTransform.GetComponent<MusicList>() == null) gameObject.AddComponent<MusicList>();
                    if (musicTransform.GetComponent<SetVolume>() == null) gameObject.AddComponent<SetVolume>();
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
            Transform audioType;
            if (string.IsNullOrWhiteSpace(section))
            {
                audioType = parentTransform.Find("Undefined");
            }
            else
            {
                audioType = parentTransform.Find(section);
            }
            if (audioType == null)
            {
                GameObject gameObject = Instantiate(new GameObject(), parentTransform);
                gameObject.name = string.IsNullOrWhiteSpace(section) != false ? gameObject.name = "Undefined" : gameObject.name = section;
                parentTransform = gameObject.transform;
            }
            else
            {
                parentTransform = audioType;
            }


            GameObject gameObject2 = Instantiate(_audioSourcePrefab, parentTransform);
            if (!string.IsNullOrWhiteSpace(name))
                gameObject2.name = name;
            gameObject2.GetComponent<AudioBlockHandler>().SetAudioInfo(section, sourcetype, this);
            AudioBlock audioBlock = new(gameObject2.name);
            audioBlock.type = type;
            audioBlock.audioSource = gameObject2.GetComponent<AudioSource>();
            audioBlock.UpdateHash();
            if (sourcetype is AudioSourceType.Sfx)
                _sfxList.Add(audioBlock);
            else if (sourcetype is AudioSourceType.Ambient)
                _ambientList.Add(audioBlock);
        }

        public void CheckAudioTree()
        {
            if (!transform.gameObject.CompareTag("AudioManager")) return; // This makes sure that this only activates inside the AudioManager prefab.
            foreach (Transform transform in transform)
            {
                List<Transform> childrenToBeMoved = new();
                foreach (Transform transform2 in transform)
                {
                    if (transform2.GetComponent<AudioSource>() != null)
                    {
                        childrenToBeMoved.Add(transform2);
                    }
                    transform2.GetComponent<AudioBlockHandler>()?.RefreshBlock(this);
                }
                Transform undefined = transform.Find("Undefined");
                if (undefined == null)
                {
                    GameObject gameObject = Instantiate(new GameObject(), transform);
                    gameObject.name = "Undefined";
                    undefined = gameObject.transform;
                }
                foreach (Transform audio in childrenToBeMoved)
                {
                    audio.SetParent(undefined);
                }
            }
        }

        public void RemoveAudioBlock(int blockHash, AudioSourceType sourceType)
        {
            Debug.LogWarning("Tset");
            if (sourceType == AudioSourceType.Sfx)
            {
                foreach(AudioBlock block in _sfxList)
                {
                    if (block.SourceHash.Equals(blockHash))
                    {
                        _sfxList.Remove(block);
                        return;
                    }
                }
            }
            if (sourceType == AudioSourceType.Ambient)
            {
                foreach (AudioBlock block in _ambientList)
                {
                    if (block.SourceHash.Equals(blockHash)) _ambientList.Remove(block);
                }
            }
        }
        public void RemoveSection(string sectionName)
        {
            foreach (Transform transform in transform)
            {
                List<Transform> childrenToBeMoved = new();
                Transform section = transform.Find(sectionName);
                if (section != null)
                {
                    if (section.gameObject.name.Equals(sectionName))
                    {
                        foreach (Transform audio in section)
                        {
                            if(audio.GetComponent<AudioBlockHandler>() != null)childrenToBeMoved.Add(audio);
                        }
                    }

                    Transform undefined = transform.Find("Undefined");
                    if (undefined == null)
                    {
                        GameObject gameObject = Instantiate(new GameObject(), transform);
                        gameObject.name = "Undefined";
                        undefined = gameObject.transform;
                    }
                    foreach (Transform audio in childrenToBeMoved)
                    {
                        audio.SetParent(undefined);
                    }
                    if (section != null) DestroyImmediate(section.gameObject);
                }
            }
        }

        public void RefreshLists()
        {
            foreach(AudioBlock block in _sfxList)
            {
                block.UpdateHash();
            }
            foreach (AudioBlock block in _ambientList)
            {
                block.UpdateHash();
            }
        }

    }

    [Serializable]
    public class AudioBlock
    {
        public string name;
        public AudioSource audioSource;
        public string type;
        private int sourceHash;

        public AudioBlock(string name)
        {
            this.name = name;
        }

        public int SourceHash { get => sourceHash;}

        public void UpdateHash()
        {
            if(audioSource != null) sourceHash = audioSource.GetHashCode();
        }
    }
    #endregion

    #region Editor Code
#if UNITY_EDITOR
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        private AudioTypeName _sourceType = AudioTypeName.Music;
        private AudioSourceType _audioSourceType = AudioSourceType.Sfx;
        private string _newName = "";
        private string _newSectionName = "";
        private string _newTypeName = "";
        [SerializeField] List<string> _audioSection = new();
        int index = 0;
        int index2 = 0;

        private void OnEnable()
        {
            if (Application.isPlaying) return;
            ((AudioManager)target).CheckAudioTree();

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            var prop = serializedObject.FindProperty("_audioSections");
            _audioSection.Clear();
            int i = 0;
            for (i = 0; i < prop.arraySize; i++)
            {
                _audioSection.Add(prop.GetArrayElementAtIndex(i).stringValue);
            }

            AudioManager script = (AudioManager)target;

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("Audio Section", EditorStyles.boldLabel);
            index= EditorGUILayout.Popup("Section", index, _audioSection.ToArray());
            _newSectionName = EditorGUILayout.TextField("Name", _newSectionName);
            if (GUILayout.Button("Add Audio Section"))
            {
                if (!string.IsNullOrWhiteSpace(_newSectionName))
                {
                    if (!_audioSection.Contains(_newSectionName.Trim())) { 
                        _audioSection.Add(_newSectionName.Trim());
                        prop.InsertArrayElementAtIndex(i);
                        prop.GetArrayElementAtIndex(i).stringValue = _newSectionName.Trim();
                    }
                }
                _newSectionName = "";
            }
            if (GUILayout.Button("Remove AudioType"))
            {
                string sectionName = _audioSection[index];
                _audioSection.Remove(sectionName);
                prop.DeleteArrayElementAtIndex(index);
                if (index != 0) index--;
                script.RemoveSection(sectionName);
                _newSectionName = "";
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
                    index2 = EditorGUILayout.Popup("Audio Section", index2, _audioSection.ToArray());
                    _newTypeName = EditorGUILayout.TextField("Type", _newTypeName);
                    break;
                case AudioSourceType.Ambient:
                    _newName = EditorGUILayout.TextField("Name", _newName);
                    index2 = EditorGUILayout.Popup("Audio Section", index2, _audioSection.ToArray());
                    _newTypeName = EditorGUILayout.TextField("Type", _newTypeName);
                    break;
            }
            if (GUILayout.Button("Add AudioSource"))
            {
                script.AddAudio(_newName, _audioSourceType, _audioSection[index2], _newTypeName);
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
    #endregion
}
