using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class MusicHandler : MonoBehaviour
    {
        [SerializeField] private MusicReference _musicReference;

        private MusicSection _currentSection = MusicSection.None;

        [SerializeField] private AudioSource _musicChannel1;
        [SerializeField] private AudioSource _musicChannel2;
        [SerializeField] private AudioSource _crossFadeEffectChannel;

        [SerializeField] private float _crossFadeDuration = 0.5f;

        private int _primaryChannel = 1;
        public int PrimaryChannel { get => _primaryChannel; }

        private float _maxVolume = 1.0f;

        private MusicCategory _currentCategory;
        private MusicTrack _currentTrack;
        private int _currentTrackIndex = 0;

        private string _mainMenuMusicName = "";
        public string MainMenuMusicName { get => _mainMenuMusicName; }

        private bool _musicSwitchInProgress = false;

        public void SetMaxVoulme(float volume) { _maxVolume = volume; }

        private void Awake()
        {
            _musicChannel1.loop = true;
            _musicChannel2.loop = true;
        }

        public List<MusicTrack> GetMusicList(string categoryName)
        {
            MusicCategory musicCategory = _musicReference.GetCategory(categoryName);

            if (musicCategory == null) return null;

            return musicCategory.MusicTracks;
        }

        public string PlayMusic(string categoryName, string trackName)
        {
            if (_musicSwitchInProgress) return null;

            _currentCategory = _musicReference.GetCategory(categoryName);

            if (_currentCategory == null) return null;

            MusicTrack musicTrack = _currentCategory.Get(trackName);

            if (musicTrack == null) return null;

            _currentTrack = musicTrack;

            if (categoryName.ToLower() == "MainMenu".ToLower()) _mainMenuMusicName = musicTrack.Name;

            SwitchMusic();

            return _currentTrack.Name;
        }

        public string PlayMusic(string categoryName, MusicTrack musicTrack)
        {
            if (_musicSwitchInProgress) return null;

            _currentCategory = _musicReference.GetCategory(categoryName);

            if (_currentCategory == null || musicTrack == null) return null;

            if (categoryName.ToLower() == "MainMenu".ToLower()) _mainMenuMusicName = musicTrack.Name;

            _currentTrack = musicTrack;
            SwitchMusic();

            return _currentTrack.Name;
        }

        public string GetTrackName()
        {
            if (_currentTrack == null) return null;

            return _currentTrack.Name;
        }

        public void StopMusic(int channel)
        {
            if (channel == 1)
                _musicChannel1.Stop();
            else
                _musicChannel2.Stop();
        }

        private List<MusicTrack> GetMusicList()
        {
            if (_currentCategory != null) return _currentCategory.MusicTracks;

            return null;
        }

        #region CrossFade

        public void SwitchMusic()
        {
            StartCoroutine(SwitchMusic(0, null));
        }

        public IEnumerator SwitchMusic(int direction, System.Action<string> newTrackName)
        {
            MusicTrack musicTrack;

            if (_currentCategory == null)
            {
                if (newTrackName != null) newTrackName("");

                yield break;
            }

            if (newTrackName != null)
            {

                _currentTrackIndex += direction;

                if (_currentTrackIndex >= _currentCategory.MusicTracks.Count)
                    _currentTrackIndex = 0;
                else if (_currentTrackIndex < 0)
                    _currentTrackIndex = _currentCategory.MusicTracks.Count - 1;

                musicTrack = _currentCategory.MusicTracks[_currentTrackIndex];
            }
            else
                musicTrack = _currentTrack;


            if (musicTrack == null)
            {
                if (newTrackName != null) newTrackName("");

                yield break;
            }

            bool? done = null;

            if (_primaryChannel == 2)
            {
                _musicChannel1.clip = musicTrack.Music;
                _musicChannel1.volume = 0f;
                _musicChannel1.Play();
            }
            else
            {
                _musicChannel2.clip = musicTrack.Music;
                _musicChannel2.volume = 0f;
                _musicChannel2.Play();
            }

            if (newTrackName != null) newTrackName(musicTrack.Name);

            _musicSwitchInProgress = true;

            StartCoroutine(CrossFadeTracks(dData => done = dData));
            yield return new WaitUntil(() => done != null);

            StopMusic(_primaryChannel);
            _primaryChannel = ((_primaryChannel == 1) ? 2 : 1);
            _currentTrack = musicTrack;
            _musicSwitchInProgress = false;
        }

        private IEnumerator CrossFadeTracks(System.Action<bool> done)
        {
            float timer = 0f;

            while (timer < _crossFadeDuration)
            {
                timer += Time.deltaTime;
                yield return null;

                float progression = timer / _crossFadeDuration;

                if (_primaryChannel == 2)
                {
                    _musicChannel1.volume = Mathf.Lerp(0f, _maxVolume, progression);
                    _musicChannel2.volume = Mathf.Lerp(_maxVolume, 0f, progression);
                }
                else
                {
                    _musicChannel1.volume = Mathf.Lerp(_maxVolume, 0f, progression);
                    _musicChannel2.volume = Mathf.Lerp(0f, _maxVolume, progression);
                }
            }

            done(true);
        }
        #endregion
    }
}
