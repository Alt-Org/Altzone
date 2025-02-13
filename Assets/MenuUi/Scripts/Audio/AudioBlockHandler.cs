using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.Audio
{
    [ExecuteInEditMode]
    public class AudioBlockHandler : MonoBehaviour
    {
        [SerializeField] private string _sectionBlock;
        [SerializeField] private AudioSourceType _sourceType;
        private int _audioSourceHash;
        private AudioManager _manager;

        private void OnDestroy()
        {
            if (Application.isPlaying) return;
            Debug.LogWarning("Tset" + _audioSourceHash + _sourceType);
            _manager.RemoveAudioBlock(_audioSourceHash, _sourceType);
        }
        private void OnTransformParentChanged()
        {
            RefreshBlock(_manager);
        }

        public void SetAudioInfo(string section, AudioSourceType sourceType, AudioManager manager)
        {
            _sectionBlock = section;
            _sourceType= sourceType;
            _audioSourceHash = GetComponent<AudioSource>().GetHashCode();
            _manager = manager;
        }

        public void RefreshBlock(AudioManager manager)
        {
            _sectionBlock = transform.parent.gameObject.name;
            _sourceType = (AudioSourceType)Enum.Parse(typeof(AudioSourceType) ,transform.parent.parent.gameObject.name.Equals("SoundFx")? "Sfx": transform.parent.parent.gameObject.name);
            _audioSourceHash = GetComponent<AudioSource>().GetHashCode();
            _manager = manager;
        }
    }
}
