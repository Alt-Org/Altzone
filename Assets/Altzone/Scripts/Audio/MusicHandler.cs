using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class MusicHandler : MonoBehaviour
    {
        private MusicSection _currentSection = MusicSection.None;
        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public string PlayMusic(MusicSection section ,int musicIndex = -1)
        {
            MusicList musicList = GetMusicList(section);

            if (musicList == null) return null;

            _currentSection = section;
            MusicObject musicObject = musicList.GetMusicObject(musicIndex);

            if (musicObject == null)
                return null;
            else
            {
                if (musicObject.MusicClip.Equals(_audioSource.clip))
                {
                    if (!_audioSource.isPlaying) _audioSource.Play();

                    return musicObject.Name;
                }

                SwitchMusic(musicObject.MusicClip);
            }

            return musicObject.Name;
        }

        public string GetTrackName()
        {
            MusicList musicList = GetMusicList(_currentSection);

            if (musicList == null) return null;

            string name = musicList.GetTrackName();

            if (name == null) return null;

            return name;
        }

        public void StopMusic()
        {
            _audioSource.Stop();
        }

        public string NextTrack()
        {
            MusicList musicList = GetMusicList(_currentSection);

            if (musicList == null) return null;

            MusicObject musicObject = musicList.NextTrack();

            if (musicObject == null)
                return null;
            else
                SwitchMusic(musicObject.MusicClip);

            return musicObject.Name;
        }

        public string PrevTrack()
        {
            MusicList musicList = GetMusicList(_currentSection);

            if (musicList == null) return null;

            MusicObject musicObject = musicList.PrevTrack();

            if (musicObject == null)
                return null;
            else
                SwitchMusic(musicObject.MusicClip);

            return musicObject.Name;
        }

        private MusicList GetMusicList(MusicSection section)
        {
            if (section == MusicSection.None) return null;

            foreach (Transform transform in transform)
            {
                MusicList currentlist = transform.GetComponent<MusicList>();

                if (currentlist != null && currentlist.MusicSection == section) return currentlist;
            }

            return null;
        }

        private void SwitchMusic(AudioClip audioClip)
        {
            StopMusic();
            _audioSource.clip = audioClip;
            _audioSource.Play();
        }
    }
}
