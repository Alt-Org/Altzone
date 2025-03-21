using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.Audio
{
    public class MusicHandler : MonoBehaviour
    {
        private MusicSection _currentSection = MusicSection.None;

        public string PlayMusic(MusicSection section ,int musicIndex = -1)
        {
            if (section == MusicSection.None) return null;
            MusicList musicList = GetMusicList(section);
            if (musicList == null) return null;
            _currentSection = section;
            MusicObject musicObject =musicList.PlayMusic(musicIndex);
            if (musicObject == null) return null;
            else
            {
                if (musicObject.MusicClip.Equals(GetComponent<AudioSource>().clip))
                {
                    if (!GetComponent<AudioSource>().isPlaying) GetComponent<AudioSource>().Play();
                    return musicObject.Name;
                }
                StopMusic();
                GetComponent<AudioSource>().clip = musicObject.MusicClip;
                GetComponent<AudioSource>().Play();
            }

            return musicObject.Name;
        }


        public string GetTrackName()
        {
            if (_currentSection == MusicSection.None) return null;
            MusicList musicList = GetMusicList(_currentSection);
            if (musicList == null) return null;
            string name =musicList.GetTrackName();
            if (name == null) return null;
            return name;
        }

        public void StopMusic()
        {
            GetComponent<AudioSource>().Stop();
        }


        public string NextTrack()
        {
            if (_currentSection == MusicSection.None) return null;
            MusicList musicList =GetMusicList(_currentSection);
            if (musicList == null) return null;
            MusicObject musicObject = musicList.NextTrack();
            if (musicObject == null) return null;
            else
            {
                StopMusic();
                GetComponent<AudioSource>().clip = musicObject.MusicClip;
                GetComponent<AudioSource>().Play();
            }
            return musicObject.Name;
        }

        public string PrevTrack()
        {
            if (_currentSection == MusicSection.None) return null;
            MusicList musicList = GetMusicList(_currentSection);
            if (musicList == null) return null;
            MusicObject musicObject = musicList.PrevTrack();
            if (musicObject == null) return null;
            else
            {
                StopMusic();
                GetComponent<AudioSource>().clip = musicObject.MusicClip;
                GetComponent<AudioSource>().Play();
            }
            return musicObject.Name;
        }

        private MusicList GetMusicList(MusicSection section)
        {
            foreach(Transform transform in transform)
            {
                MusicList currentlist = transform.GetComponent<MusicList>();
                if (currentlist != null)
                {
                    if (currentlist.MusicSection == section) return currentlist;
                }
            }
            return null;
        }
    }
}
