using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Audio
{
    public enum MusicState  // Using same values as in MyGamez to avoid confusion.
    {
        OFF = 0,            // Music off.
        ON = 1,             // Music on.
        INDETERMINATE = 2,  // Use current save game setting for music.
    }

    public class AudioManager : MonoBehaviour
    {
	    private static AudioManager _instance;

	    static AudioManager Instance => _instance;

		private HashSet<AudioPlayer> _backMusics;
		private HashSet<AudioPlayer> _UIEffects;
		private HashSet<AudioPlayer> _gameEffects;

		//default master volume settings
		private float _musicVolume 		= 1;
		private float _effectVolume 	= 1;

		private AudioClip _buttonBeep;
        private Dictionary<string, AudioClip> effect2D = new Dictionary<string, AudioClip>();   // Cached 2D audio effects.

        protected void Awake(){

			_backMusics = new HashSet<AudioPlayer>();
			_UIEffects = new HashSet<AudioPlayer>();
			_gameEffects = new HashSet<AudioPlayer>();

			_buttonBeep = ResourceLoader.Load<AudioClip>("Audio/UIButtonBeep");

            int musicState = PlayerPrefs.GetInt("MusicState", (int)MusicState.INDETERMINATE);
			SetMusicState ((MusicState)musicState);
		}

		public static void RegisterAudio(AudioPlayer audio){

			HashSet<AudioPlayer> collection = _instance.giveTrackCollection(audio.Track);

			if(collection != null && !collection.Contains(audio)){
				float vol = _instance.getTrackVolume(audio.Track);

				collection.Add(audio);
				audio.Source.volume = audio.InitialVolume * vol;

			}
		}

		public static void UnregisterAudio(AudioPlayer audio){
			if(_instance != null){
				HashSet<AudioPlayer> collection = _instance.giveTrackCollection(audio.Track);

				if(collection != null && collection.Contains(audio))
					collection.Remove(audio);
			}
		}

		private static void SetMusicState(MusicState state){
			// Hack to set master volume.
			var masterVol = PlayerPrefs.GetFloat("MasterVolume",1f);
			AudioListener.volume = Mathf.Clamp(masterVol,0f,1f);
			PlayerPrefs.SetInt ("MusicState", (int)state);
            //XDebug.Log("SetMusicState={0} MasterVolume={1:0.0}", state, masterVol);

			//force disable audio
			if (state == MusicState.OFF) {
				SetMusicVolume (0.0f);
				SetUIVolume (0.0f);
				SetEffectsVolume (0.0f);
				PlayerPrefs.SetFloat("Music", 0);
				PlayerPrefs.SetFloat("SoundFX", 0);
			}

			//force enable audio
			else if (state == MusicState.ON) {
				SetMusicVolume (_instance._musicVolume);
				SetUIVolume (_instance._effectVolume);
				SetEffectsVolume (_instance._effectVolume);
				PlayerPrefs.SetFloat("Music", _instance._musicVolume);
				PlayerPrefs.SetFloat("SoundFX", _instance._effectVolume);
			}

			//read previous settings
			else if (state == MusicState.INDETERMINATE) {
				float musicVol = PlayerPrefs.GetFloat("Music", _instance._musicVolume);
				float soundVol = PlayerPrefs.GetFloat("SoundFX", _instance._effectVolume);
				SetMusicVolume(musicVol);
				SetUIVolume(soundVol);
				SetEffectsVolume(soundVol);
                //XDebug.Log("Music={0:0.0} SoundFX={1:0.0}", musicVol, soundVol);
            }
		}

		public static void SetMusicVolume(float vol){
			_instance._musicVolume = Mathf.Clamp(vol,0,1);
			_instance.setVolume(_instance._backMusics, _instance._musicVolume);
		}

		public static void SetEffectsVolume(float vol){
			_instance._effectVolume = Mathf.Clamp(vol,0,1);
			_instance.setVolume(_instance._gameEffects, _instance._effectVolume);
			_instance.setVolume(_instance._UIEffects, _instance._effectVolume);
		}

		private static void SetUIVolume(float vol){
			// Shared with effects volume!
		}

		private static void PlayAudio(AudioClip clip, AudioClipTrack asType, Vector3 atPoint){
			AudioSource.PlayClipAtPoint(clip, atPoint, Instance.getTrackVolume(asType));
		}

		public static void PlayAudioBeep(){

			PlayAudio(Instance._buttonBeep, AudioClipTrack.UI, Vector3.zero);
		}

        public static void RegisterAudioEffect2D(string audioClipName)
        {
            if (!Instance.effect2D.ContainsKey(audioClipName))
            {
                AudioClip audioClip = ResourceLoader.Load<AudioClip>(audioClipName);
                Instance.effect2D.Add(audioClipName, audioClip);
            }
        }

        public static void PlayAudioEffect2D(string audioClipName, Vector3 atPoint)
        {
            AudioClip audioClip;
            if (Instance.effect2D.TryGetValue(audioClipName, out audioClip))
            {
                PlayAudio(audioClip, AudioClipTrack.UI, atPoint);
            }
        }

        private void setVolume(HashSet<AudioPlayer> collection, float newVol){
			AudioPlayer[] audio = new AudioPlayer[collection.Count];
			collection.CopyTo(audio);

			for(int i=0; i<audio.Length; i++)
				audio[i].Source.volume = audio[i].InitialVolume * newVol;

		}

		private float getTrackVolume(AudioClipTrack track){

			switch(track){
				case AudioClipTrack.MUSIC:
					return _musicVolume;
				case AudioClipTrack.EFFECT:
					return _effectVolume;
				case AudioClipTrack.UI:
					return _effectVolume;
				default:
					return 1;
			}
		}

		private HashSet<AudioPlayer> giveTrackCollection(AudioClipTrack track){

			switch(track){
				case AudioClipTrack.MUSIC:
					return _backMusics;
				case AudioClipTrack.EFFECT:
					return _gameEffects;
				case AudioClipTrack.UI:
					return _UIEffects;
				default:
					return null;
			}
		}
    }

    public class ResourceLoader : MonoBehaviour
    {

	    private static Dictionary<string, Object> _resourceCache = new Dictionary<string, Object>();

	    public static T Load<T>(string path) where T : Object
	    {

		    if (path == null || path.Length == 0)
			    return null;

		    if (!_resourceCache.ContainsKey(path))
		    {

			    T resource = Resources.Load<T>(path);
			    if (resource == null)
				    return null;
			    else
				    _resourceCache.Add(path, resource);

		    }
		    return (T)_resourceCache[path];
	    }


	    public static void Unload(string path)
	    {

		    if (_resourceCache.ContainsKey(path))
		    {
			    Resources.UnloadAsset(_resourceCache[path]);
			    _resourceCache.Remove(path);
		    }
	    }

	    public static void Unload(string[] paths)
	    {

		    for (int i = 0; i < paths.Length; i++)
			    Unload(paths[i]);
	    }

	    public static void UnloadAll()
	    {

		    foreach (KeyValuePair<string, Object> kv in _resourceCache)
			    Resources.UnloadAsset(kv.Value);

		    _resourceCache.Clear();
	    }
    }
}