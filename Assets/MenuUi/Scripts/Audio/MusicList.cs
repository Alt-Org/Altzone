using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.Audio
{
    public enum MusicSection
    {
        None,
        MainMenu,
        Battle,
        SoulHome,
        All
    }

    [System.Serializable]
    public class MusicObject
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private AudioClip _musicClip;

        public AudioClip MusicClip { get => _musicClip;}
        public string Name { get => _name;}
    }

    public class MusicList : MonoBehaviour
    {
        [SerializeField]
        private List<MusicObject> _musicList = new();
        [SerializeField]
        private MusicSection _musicSection;

        public List<MusicObject> Music { get => _musicList;}

        public MusicSection MusicSection { get => _musicSection; }

        private int _musicTrack = 0;
        [SerializeField]
        private int _defaultMusicTrack = 0;

        // Start is called before the first frame update
        void Start()
        {
            _musicTrack = _defaultMusicTrack;
        }

        public MusicObject PlayMusic(int musicIndex = -1)
        {
            if (_musicList.Count == 0 || _musicList.Count <= musicIndex) return null;
            if (musicIndex >= 0) _musicTrack = musicIndex;
            /*if (_musicList[_musicTrack].MusicClip != null)
            {
                GetComponent<AudioSource>().clip =_musicList[_musicTrack].MusicClip;
                GetComponent<AudioSource>().Play();
            }*/
            return _musicList[_musicTrack];
        }

        public void StopMusic()
        {
            GetComponent<AudioSource>().Stop();
        }

        public string GetTrackName()
        {
            if (_musicList.Count == 0) return null;
            return _musicList[_musicTrack].Name;
        }

        public MusicObject NextTrack()
        {
            if (_musicList.Count < 2) return null;
            _musicTrack++;
            if (_musicTrack >= _musicList.Count) _musicTrack = 0;

            return _musicList[_musicTrack];
        }

        public MusicObject PrevTrack()
        {
            if (_musicList.Count < 2) return null;

            _musicTrack--;
            if (_musicTrack < 0) _musicTrack = _musicList.Count - 1;

            return _musicList[_musicTrack];
        }

    }
}
