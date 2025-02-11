using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.Audio
{
    public class AudioBlockHandler : MonoBehaviour
    {
        [SerializeField] private string _sectionBlock;
        [SerializeField] private AudioSourceType _sourceType;

        [ExecuteInEditMode]
        private void OnDestroy()
        {
            Debug.LogWarning("Tset");
            AudioManager.Instance.RemoveAudioBlock(GetComponent<AudioSource>(), _sourceType);
        }

        public void SetAudioInfo(string section, AudioSourceType sourceType)
        {
            _sectionBlock = section;
            _sourceType= sourceType;
        }
    }
}
