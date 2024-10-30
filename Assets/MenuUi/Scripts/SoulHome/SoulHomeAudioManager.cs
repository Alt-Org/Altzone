using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public enum AudioType
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

    public class SoulHomeAudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _musicAudio;
        private MusicList _musicList;
        [SerializeField] private List<AudioBlock> _audioList;

        [SerializeField] private GameObject _audioSourcePrefab;
        // Start is called before the first frame update
        void Start()
        {
            if (_musicAudio != null)
            {
                _musicList = _musicAudio.GetComponent<MusicList>();
            }
        }

        public void PlayAudio(AudioType type)
        {
            if (type == AudioType.None) return;

            if (type == AudioType.Music) PlayMusic();

            foreach(AudioBlock block in _audioList)
            {
                if(block.type == type) block.audioSource.Play();
            }
        }

        public void PlayAudio(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            if (string.Equals(name, "Music")) PlayMusic();

            foreach (AudioBlock block in _audioList)
            {
                if (string.Equals(name, block.name)) block.audioSource.Play();
            }
        }
        public void PlayAudio(int value)
        {
            if (value < 1) return;

            if (value == 1) PlayMusic();

            if (_audioList.Count < value-2)
            {
                _audioList[value-2].audioSource.Play();
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

        public void AddAudio(string name, AudioType type)
        {
            GameObject gameObject = Instantiate(_audioSourcePrefab, transform);
            if (!string.IsNullOrWhiteSpace(name))
                gameObject.name = name;
            AudioBlock audioBlock = new(gameObject.name);
            audioBlock.type = type;
            audioBlock.audioSource = gameObject.GetComponent<AudioSource>();
            _audioList.Add(audioBlock);
        }
    }

    [Serializable]
    public class AudioBlock
    {
        [ReadOnly] public string name;
        public AudioSource audioSource;
        public AudioType type;

        public AudioBlock(string name)
        {
            this.name = name;
        }
    }

    [CustomEditor(typeof(SoulHomeAudioManager))]
    public class AudioManagerEditor : Editor
    {
        private AudioType _type = AudioType.Music;
        private string _newName = "";
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            SoulHomeAudioManager script = (SoulHomeAudioManager)target;
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("New AudioSource", EditorStyles.boldLabel);
            _newName = EditorGUILayout.TextField("Name", _newName);
            _type = (AudioType)EditorGUILayout.EnumPopup("Audio Type",_type);
            if (GUILayout.Button("Add AudioSource"))
            {
                script.AddAudio(_newName, _type);
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
