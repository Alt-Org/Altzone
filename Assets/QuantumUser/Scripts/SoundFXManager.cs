using UnityEngine;

namespace QuantumUser.Scripts
{
    public class SoundFXManager : MonoBehaviour
    {
        public static SoundFXManager instance;

        [SerializeField] private AudioSource soundFXObject;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }


        public void PlaySoundFXclip(AudioClip audioClip, float volume)
        {
            //spawn sound gameobject
            AudioSource audioSource = Instantiate(soundFXObject);
            //assign audioclip
            audioSource.clip = audioClip;
            //assign volume
            audioSource.volume = volume;
            //play sound
            audioSource.Play();
            //get length of sound FX clip
            float clipLength = audioSource.clip.length;
            //destroy sound
            Destroy(audioSource.gameObject,clipLength);
        }

    }
}
