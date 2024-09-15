using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public enum AudioType
    {
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
        [SerializeField] private AudioSource _clickAudio;
        [SerializeField] private AudioSource _popupErrorAudio;
        [SerializeField] private AudioSource _revertAudio;
        [SerializeField] private AudioSource _saveAudio;
        [SerializeField] private AudioSource _rotateAudio;
        [SerializeField] private AudioSource _setFurnitureAudio;

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
            switch (type)
            {
                case AudioType.Music:
                    _musicList.PlayMusic();
                    break;
                case AudioType.Click:
                    _clickAudio.Play();
                    break;
                case AudioType.PopupError:
                    _popupErrorAudio.Play();
                    break;
                case AudioType.Revert:
                    _revertAudio.Play();
                    break;
                case AudioType.Save:
                    _saveAudio.Play();
                    break;
                case AudioType.Rotate:
                    _rotateAudio.Play();
                    break;
                case AudioType.SetFurniture:
                    _setFurnitureAudio.Play();
                    break;
            }
        }

        public string PlayMusic()
        {
            if (_musicList = null) return null;
            return _musicList.PlayMusic();
        }

        public string NextMusicTrack()
        {
            if (_musicList = null) return null;
            return _musicList.NextTrack();
        }

        public string PrevMusicTrack()
        {
            if(_musicList = null) return null;
            return _musicList.PrevTrack();
        }

        public void StopMusic()
        {
            if (_musicList = null) return;
            _musicList.StopMusic();
        }
    }
}
