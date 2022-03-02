using UnityEngine;

namespace Battle.Scripts.Audio
{
    public enum AudioClipTrack{NONE, MUSIC, EFFECT, UI}

    [RequireComponent (typeof (AudioSource))]
    public class AudioPlayer : MonoBehaviour {



        public AudioClipTrack Track{ get{return _track;}}
        public AudioSource Source{get{ return _source; }}
        public float InitialVolume{get{return _initialVolume; }}

        [SerializeField]
        protected AudioClipTrack _track;
        protected AudioSource _source;
        protected float _initialVolume;

        protected virtual void Awake(){

            _source = GetComponent<AudioSource>();
            _initialVolume = _source.volume;

            if(_track == AudioClipTrack.NONE){
                Debug.LogError("AudioPlayer "+name+" ( clip: "+_source.clip.name+") doesn't have a track selected. Did you set it from the inspector?");
                _source.volume = 0;
            }
            else
                AudioManager.RegisterAudio(this);
        }

        protected virtual void OnDestroy(){
            AudioManager.UnregisterAudio(this);
            Destroy(_source);

        }

    }
}