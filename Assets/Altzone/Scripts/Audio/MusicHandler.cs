using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class MusicHandler : MonoBehaviour
    {
        [SerializeField] private MusicReference _musicReference;

        private MusicSection _currentSection = MusicSection.None;

        [SerializeField] private AudioSource _musicChannel1;
        [SerializeField] private AudioSource _musicChannel2;

        [SerializeField] private float _crossFadeDuration = 2f;
        [SerializeField] private float _acceleratedCrossFadeDuration = 0.75f;

        [SerializeField] private AnimationCurve _crossFadeCurve;

        private int _primaryChannel = 1;
        public int PrimaryChannel { get => _primaryChannel; }

        private float _maxVolume = 1.0f;

        private MusicCategory _currentCategory;
        public MusicCategory CurrentCategory {  get => _currentCategory; }

        private MusicTrack _currentTrack;
        public MusicTrack CurrentTrack { get => _currentTrack; }

        private int _currentTrackIndex = 0;

        private MusicCategory _nextUpCategory;
        private MusicTrack _nextUpTrack;

        private float _crossFadeTimer = 0f;

        private string _mainMenuMusicName = "";
        public string MainMenuMusicName { get => _mainMenuMusicName; }

        private bool _musicSwitchInProgress = false;

        private float _musicStartTime = 0f;

        public enum MusicListDirection
        {
            Next,
            Previous,
            None
        }

        public enum MusicSwitchType
        {
            Immediate,
            CrossFade
        }

        public void SetMaxVolume(float volume)
        {
            _maxVolume = volume;

            if (_primaryChannel == 1)
                _musicChannel1.volume = volume;
            else
                _musicChannel2.volume = volume;
        }

        private void Awake()
        {
            _musicChannel1.loop = true;
            _musicChannel2.loop = true;

            _musicChannel1.volume = 0f;
            _musicChannel2.volume = 0f;
        }

        public List<MusicTrack> GetMusicList(string categoryName)
        {
            MusicCategory musicCategory = _musicReference.GetCategory(categoryName);

            if (musicCategory == null) return null;

            return musicCategory.MusicTracks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusicById(string categoryName, string trackId, MusicSwitchType switchType)
        {
            MusicCategory currentCategory = _musicReference.GetCategory(categoryName);

            if (currentCategory == null) return null;

            MusicTrack musicTrack = currentCategory.GetById(trackId);

            if (musicTrack == null) return null;

            if (categoryName.ToLower() == "MainMenu".ToLower())
                _mainMenuMusicName = SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic);

            SwitchMusic(currentCategory, musicTrack, switchType);

            return musicTrack.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(string categoryName, string trackName, MusicSwitchType switchType)
        {
            MusicCategory currentCategory = _musicReference.GetCategory(categoryName);

            if (currentCategory == null) return null;

            if (categoryName.ToLower() == "MainMenu".ToLower())
                _mainMenuMusicName = SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic);

            if (categoryName.ToLower() == "MainMenu".ToLower() && string.IsNullOrEmpty(trackName)) trackName = _mainMenuMusicName;

            MusicTrack musicTrack = currentCategory.Get(trackName);

            if (musicTrack == null) return null;

            SwitchMusic(currentCategory, musicTrack, switchType);
            
            return musicTrack.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(string categoryName, MusicTrack musicTrack, MusicSwitchType switchType)
        {
            MusicCategory currentCategory = _musicReference.GetCategory(categoryName);

            if (currentCategory == null || musicTrack == null) return null;

            if (categoryName.ToLower() == "MainMenu".ToLower())
                _mainMenuMusicName = SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic);

            SwitchMusic(currentCategory, musicTrack, switchType);

            return musicTrack.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(string categoryName, string trackName, MusicSwitchType switchType, float startLocation)
        {
            _musicStartTime = startLocation;
            return PlayMusic(categoryName, trackName, switchType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(string categoryName, MusicTrack musicTrack, MusicSwitchType switchType, float startLocation)
        {
            _musicStartTime = startLocation;
            return PlayMusic(categoryName, musicTrack, switchType);
        }

        public string GetTrackName()
        {
            if (_currentTrack == null) return null;

            return _currentTrack.Name;
        }

        public void StopMusic(int channel)
        {
            if (channel == 1)
            {
                _musicChannel1.Stop();
                _musicChannel1.clip = null;
            }
            else
            {
                _musicChannel2.Stop();
                _musicChannel2.clip = null;
            }

            _currentTrack = null;
        }

        public List<MusicTrack> GetMusicList()
        {
            if (_currentCategory != null) return _currentCategory.MusicTracks;

            return null;
        }

        public void SwitchMusic(MusicCategory musicCategory, MusicTrack musicTrack, MusicSwitchType switchType)
        {
            if (_currentTrack == musicTrack) return;

            if (_currentCategory != null && _currentCategory.Name.ToLower() == "Jukebox".ToLower() && musicCategory.Name.ToLower() != "Jukebox".ToLower())
                JukeboxManager.Instance.StopJukebox();

            if (_musicSwitchInProgress)
            {
                if (_nextUpTrack != null) CalculateAcceleratedResumeTime(); //Wrong?

                _nextUpCategory = musicCategory;
                _nextUpTrack = musicTrack;
                return;
            }

            _currentCategory = musicCategory;
            _currentTrack = musicTrack;
            StartCoroutine(SwitchMusic(MusicListDirection.None, null, switchType));
        }

        public IEnumerator SwitchMusic(MusicListDirection direction, System.Action<string> newTrackName, MusicSwitchType switchType)
        {
            MusicTrack musicTrack;

            if (_currentCategory == null)
            {
                if (newTrackName != null) newTrackName("");

                yield break;
            }

            if (newTrackName != null && direction != MusicListDirection.None)
            {
                if (direction == MusicListDirection.Next)
                    _currentTrackIndex += 1;
                else
                    _currentTrackIndex -= 1;

                if (_currentTrackIndex >= _currentCategory.MusicTracks.Count)
                    _currentTrackIndex = 0;
                else if (_currentTrackIndex < 0)
                    _currentTrackIndex = _currentCategory.MusicTracks.Count - 1;

                musicTrack = _currentCategory.MusicTracks[_currentTrackIndex];
            }
            else
                musicTrack = _currentTrack;

            if (musicTrack == null || _musicSwitchInProgress)
            {
                if (newTrackName != null) newTrackName("");

                if (_musicSwitchInProgress)
                {
                    if (_nextUpTrack == null) CalculateAcceleratedResumeTime();

                    _nextUpCategory = _currentCategory;
                    _nextUpTrack = musicTrack;
                }

                yield break;
            }

            if (newTrackName != null) newTrackName(musicTrack.Name);

            bool? done = null;

            _musicSwitchInProgress = true;
            StartMusicPlayback((_primaryChannel == 2 ? _musicChannel1 : _musicChannel2), musicTrack.Music);

            if (switchType == MusicSwitchType.CrossFade)
                StartCoroutine(CrossFadeTracks(dData => done = dData, _primaryChannel));
            else
                SwitchTracksImmediately(dData => done = dData, _primaryChannel);

            yield return new WaitUntil(() => done != null);

            StopMusic(_primaryChannel);
            _primaryChannel = ((_primaryChannel == 1) ? 2 : 1);
            _currentTrack = musicTrack;
            _musicSwitchInProgress = false;

            if (_nextUpTrack != null) //Play a waiting track.
            {
                _currentCategory = _nextUpCategory;
                _currentTrack = _nextUpTrack;

                _nextUpCategory = null;
                _nextUpTrack = null;

                StartCoroutine(SwitchMusic(MusicListDirection.None, null, switchType));
            }
        }

        private void CalculateAcceleratedResumeTime()
        {
            _crossFadeTimer = _acceleratedCrossFadeDuration * (_crossFadeTimer / _crossFadeDuration);
        }

        private void StartMusicPlayback(AudioSource source, AudioClip audio)
        {
            if (_musicStartTime >= audio.length || _musicStartTime < 0)
            {
                Debug.LogWarning("_musicStartTime is out of range! value: " + _musicStartTime);
                _musicStartTime = 0f;
            }

            source.clip = audio;
            source.time = _musicStartTime;
            _musicStartTime = 0f;
            source.Play();
        }

        private IEnumerator CrossFadeTracks(System.Action<bool> done, int primaryChannel)
        {
            _crossFadeTimer = 0f;

            while (_crossFadeTimer < _crossFadeDuration)
            {
                yield return null;

                float progression;

                if (_nextUpCategory == null)
                    progression = _crossFadeCurve.Evaluate(_crossFadeTimer / _crossFadeDuration);
                else
                    progression = _crossFadeCurve.Evaluate(_crossFadeTimer / _acceleratedCrossFadeDuration);

                if (primaryChannel == 2)
                {
                    _musicChannel1.volume = Mathf.Lerp(0f, _maxVolume, progression);
                    _musicChannel2.volume = Mathf.Lerp(_maxVolume, 0f, progression);
                }
                else
                {
                    _musicChannel1.volume = Mathf.Lerp(_maxVolume, 0f, progression);
                    _musicChannel2.volume = Mathf.Lerp(0f, _maxVolume, progression);
                }

                _crossFadeTimer += Time.deltaTime;
            }

            done(true);
        }

        private void SwitchTracksImmediately(System.Action<bool> done, int primaryChannel)
        {
            if (primaryChannel == 2)
            {
                _musicChannel1.volume = _maxVolume;
                _musicChannel2.volume = 0f;
            }
            else
            {
                _musicChannel1.volume = 0f;
                _musicChannel2.volume = _maxVolume;
            }

            done(true);
        }
    }
}
