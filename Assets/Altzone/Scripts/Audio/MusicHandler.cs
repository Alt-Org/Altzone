using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class MusicHandler : MonoBehaviour
    {
        public static MusicHandler Instance;

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
        public float MaxVolume { get { return _maxVolume; } }

        private MusicCategory _currentCategory;
        public MusicCategory CurrentCategory {  get => _currentCategory; }

        private MusicTrack _currentTrack;
        public MusicTrack CurrentTrack { get => _currentTrack; }

        private int _currentTrackIndex = 0;

        private MusicCategory _nextUpCategory;
        private MusicTrack _nextUpTrack;

        private float _crossFadeTimer = 0f;
        private bool _musicSwitchInProgress = false;
        private bool _acceleratedCrossFadeOneShot = false;

        private string _mainMenuMusicName = "";
        public string MainMenuMusicName { get => _mainMenuMusicName; }

        private float _musicStartTime = 0f;

        private Coroutine _switchCoroutine;
        private Coroutine _crossfadeCoroutine;

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
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            _musicChannel1.loop = true;
            _musicChannel2.loop = true;
            
            _musicChannel1.volume = 0f;
            _musicChannel2.volume = 0f;
        }

        public void SetMainMenuMusicName(string name) { _mainMenuMusicName = name; }

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
        public string PlayMusicById(string categoryName, string trackId, MusicSwitchType switchType, bool forcePlay = false)
        {
            MusicCategory currentCategory = _musicReference.GetCategory(categoryName);

            if (currentCategory == null) return null;

            MusicTrack musicTrack = currentCategory.GetById(trackId);

            if (musicTrack == null) return null;

            if (categoryName.ToLower() == "MainMenu".ToLower())
                _mainMenuMusicName = SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic);

            _musicStartTime = 0f;
            SwitchMusic(currentCategory, musicTrack, switchType, forcePlay);

            return musicTrack.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusicById(AudioCategoryType categoryType, string trackId, MusicSwitchType switchType, bool forcePlay = false)
        {
            MusicCategory currentCategory = _musicReference.GetCategory(categoryType);

            if (currentCategory == null) return null;

            MusicTrack musicTrack = currentCategory.GetById(trackId);

            if (musicTrack == null) return null;

            if (categoryType == AudioCategoryType.MainMenu)
                _mainMenuMusicName = SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic);

            _musicStartTime = 0f;
            SwitchMusic(currentCategory, musicTrack, switchType, forcePlay);

            return musicTrack.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(string categoryName, string trackName, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            _musicStartTime = startLocation;
            return PlayMusic(categoryName, trackName, switchType, forcePlay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, string trackName, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            _musicStartTime = startLocation;
            return PlayMusic(categoryType, trackName, switchType, forcePlay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(string categoryName, MusicTrack musicTrack, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            _musicStartTime = startLocation;
            return PlayMusic(categoryName, musicTrack, switchType, forcePlay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, MusicTrack musicTrack, MusicSwitchType switchType, float startLocation, bool forcePlay = false)
        {
            _musicStartTime = startLocation;
            return PlayMusic(categoryType, musicTrack, switchType, forcePlay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(string categoryName, string trackName, MusicSwitchType switchType, bool forcePlay = false)
        {
            MusicCategory currentCategory = _musicReference.GetCategory(categoryName);

            if (currentCategory == null) return null;

            if (categoryName.ToLower() == "MainMenu".ToLower())
            {
                _mainMenuMusicName = SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic);

                if (string.IsNullOrEmpty(trackName)) trackName = _mainMenuMusicName;
            }

            MusicTrack musicTrack = currentCategory.Get(trackName);

            if (musicTrack == null) return null;

            _musicStartTime = 0f;
            SwitchMusic(currentCategory, musicTrack, switchType, forcePlay);
            
            return musicTrack.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, string trackName, MusicSwitchType switchType, bool forcePlay = false)
        {
            MusicCategory currentCategory = _musicReference.GetCategory(categoryType);

            if (currentCategory == null) return null;

            if (categoryType == AudioCategoryType.MainMenu)
            {
                _mainMenuMusicName = SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic);

                if (string.IsNullOrEmpty(trackName)) trackName = _mainMenuMusicName;
            }

            MusicTrack musicTrack = currentCategory.Get(trackName);

            if (musicTrack == null) return null;

            _musicStartTime = 0f;
            SwitchMusic(currentCategory, musicTrack, switchType, forcePlay);

            return musicTrack.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(string categoryName, MusicTrack musicTrack, MusicSwitchType switchType, bool forcePlay = false)
        {
            MusicCategory currentCategory = _musicReference.GetCategory(categoryName);

            if (currentCategory == null || musicTrack == null) return null;

            if (categoryName.ToLower() == "MainMenu".ToLower())
                _mainMenuMusicName = SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic);

            SwitchMusic(currentCategory, musicTrack, switchType, forcePlay);

            return musicTrack.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Track name if successfully started the track playback.</returns>
        public string PlayMusic(AudioCategoryType categoryType, MusicTrack musicTrack, MusicSwitchType switchType, bool forcePlay = false)
        {
            MusicCategory currentCategory = _musicReference.GetCategory(categoryType);

            if (currentCategory == null || musicTrack == null) return null;

            if (categoryType == AudioCategoryType.MainMenu)
                _mainMenuMusicName = SettingsCarrier.Instance.GetSelectionBoxData(SettingsCarrier.SelectionBoxType.MainMenuMusic);

            SwitchMusic(currentCategory, musicTrack, switchType, forcePlay);

            return musicTrack.Name;
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

        public void SwitchMusic(MusicCategory musicCategory, MusicTrack musicTrack, MusicSwitchType switchType, bool forcePlay)
        {
            if (_currentTrack == musicTrack && !forcePlay) return;

            if (_currentCategory != null && _currentCategory.Name.ToLower() == "Jukebox".ToLower() && musicCategory.Name.ToLower() != "Jukebox".ToLower())
                JukeboxManager.Instance.StopJukebox();

            bool revertSwitch = (!forcePlay && (_primaryChannel == 1 ? _musicChannel1 : _musicChannel2).clip == musicTrack.Music);

            if (!revertSwitch && _musicSwitchInProgress)
            {
                HandleNextTrack(musicTrack);

                return;
            }

            _currentCategory = musicCategory;
            _currentTrack = musicTrack;

            if (_switchCoroutine != null)
            {
                StopCoroutine(_switchCoroutine);
                _switchCoroutine = null;
                
                if (_musicSwitchInProgress) _primaryChannel = ((_primaryChannel == 1) ? 2 : 1);

                if (revertSwitch) _crossFadeTimer = (_acceleratedCrossFadeOneShot ? _acceleratedCrossFadeDuration - _crossFadeTimer : _crossFadeDuration - _crossFadeTimer);
            }

            _switchCoroutine = StartCoroutine(SwitchMusic(MusicListDirection.None, null, switchType, revertSwitch));
        }

        public IEnumerator SwitchMusic(MusicListDirection direction, System.Action<string> newTrackName, MusicSwitchType switchType, bool revertSwitch = false)
        {
            if (_currentCategory == null)
            {
                if (newTrackName != null) newTrackName("");

                yield break;
            }

            MusicTrack musicTrack = (newTrackName != null ? MusicDirectionControl(newTrackName, direction) : _currentTrack);
            //MusicTrack musicTrack = MusicDirectionControl(newTrackName, direction);

            if (!revertSwitch && (musicTrack == null || _musicSwitchInProgress))
            {
                if (newTrackName != null) newTrackName("");

                if (_musicSwitchInProgress) HandleNextTrack(musicTrack);

                yield break;
            }

            bool? done = null;

            if (newTrackName != null) newTrackName(musicTrack.Name);

            _musicSwitchInProgress = true;
            
            if (!revertSwitch) StartMusicPlayback((_primaryChannel == 2 ? _musicChannel1 : _musicChannel2), musicTrack.Music);

            if (switchType == MusicSwitchType.CrossFade)
            {
                if (_crossfadeCoroutine != null)
                {
                    StopCoroutine(_crossfadeCoroutine);
                    _crossfadeCoroutine = null;
                }

                _crossfadeCoroutine = StartCoroutine(CrossFadeTracks(dData => done = dData, _primaryChannel, revertSwitch));
            }
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

                _switchCoroutine = StartCoroutine(SwitchMusic(MusicListDirection.None, null, switchType));
            }
            else
                _acceleratedCrossFadeOneShot = false;
        }

        private void HandleNextTrack(MusicTrack musicTrack)
        {
            _nextUpCategory = _currentCategory;
            _nextUpTrack = musicTrack;

            if (!_acceleratedCrossFadeOneShot && _nextUpTrack != null)
            {
                CalculateAcceleratedResumeTime();
                _acceleratedCrossFadeOneShot = true;
            }
        }

        private MusicTrack MusicDirectionControl(System.Action<string> newTrackName, MusicListDirection direction)
        {
            if (newTrackName == null || direction == MusicListDirection.None) return _currentTrack;

            if (direction == MusicListDirection.Next)
                _currentTrackIndex += 1;
            else
                _currentTrackIndex -= 1;

            if (_currentTrackIndex >= _currentCategory.MusicTracks.Count)
                _currentTrackIndex = 0;
            else if (_currentTrackIndex < 0)
                _currentTrackIndex = _currentCategory.MusicTracks.Count - 1;

            return _currentCategory.MusicTracks[_currentTrackIndex];
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

        private IEnumerator CrossFadeTracks(System.Action<bool> done, int primaryChannel, bool revert)
        {
            if (!revert) _crossFadeTimer = 0f;

            while (_nextUpCategory == null ? _crossFadeTimer < _crossFadeDuration : _crossFadeTimer < _acceleratedCrossFadeDuration)
            {
                yield return null;

                float progression;

                if (!_acceleratedCrossFadeOneShot)
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
