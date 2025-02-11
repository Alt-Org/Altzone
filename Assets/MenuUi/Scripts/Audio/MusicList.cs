using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.Audio
{
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

        public List<MusicObject> Music { get => _musicList;}

        private int _musicTrack = 0;
        [SerializeField]
        private int _defaultMusicTrack = 0;

        // Start is called before the first frame update
        void Start()
        {
            _musicTrack = _defaultMusicTrack;
        }

        public string PlayMusic()
        {
            if (_musicList.Count == 0) return null;
            if (_musicList[_musicTrack].MusicClip != null)
            {
                GetComponent<AudioSource>().clip =_musicList[_musicTrack].MusicClip;
                GetComponent<AudioSource>().Play();
            }
            return GetTrackName();
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

        public string NextTrack()
        {
            if (_musicList.Count < 2) return null;
            StopMusic();
            _musicTrack++;
            if (_musicTrack >= _musicList.Count) _musicTrack = 0;

            PlayMusic();
            return _musicList[_musicTrack].Name;
        }

        public string PrevTrack()
        {
            if (_musicList.Count < 2) return null;
            StopMusic();
            _musicTrack--;
            if (_musicTrack < 0) _musicTrack = _musicList.Count - 1;

            PlayMusic();
            return _musicList[_musicTrack].Name;
        }

    }
}
