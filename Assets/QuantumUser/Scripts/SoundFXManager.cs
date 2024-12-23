using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuantumUser.Scripts
{
    public class SoundFXManager : MonoBehaviour
    {
        public static SoundFXManager instance;

        [SerializeField] private AudioSource _audioSource; // Reference to a preconfigured AudioSource

        [SerializeField] private AudioClip _soulWallHitClip;
        [SerializeField] private AudioClip _goalHitClip;
        [SerializeField] private AudioClip _sideWallHitClip;
        [SerializeField] private AudioClip _wallBroken;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void OnEnable()
        {
            QuantumEvent.Subscribe<EventPlaySoundEvent>(this, OnPlaySoundEvent);
        }

        private void OnDisable()
        {
            //QuantumEvent.Unsubscribe<EventPlaySoundEvent>(this, OnPlaySoundEvent);
        }

        private void OnPlaySoundEvent(EventPlaySoundEvent e)
        {
            // Map SoundEffect enum to the correct AudioClip
            AudioClip clip = e.SoundEffect switch
            {
                SoundEffect.SoulWallHit => _soulWallHitClip,
                SoundEffect.GoalHit     => _goalHitClip,
                SoundEffect.SideWallHit => _sideWallHitClip,
                SoundEffect.WallBroken  => _wallBroken,
                _ => null,
            };

            if (clip == null)
            {
                Debug.LogWarning("Unhandled sound effect: " + e.SoundEffect);
                return;
            }

            PlaySoundFXclip(clip);
        }

        private void PlaySoundFXclip(AudioClip clip)
        {
            if (_audioSource != null)
            {
                _audioSource.PlayOneShot(clip); // Play the sound without affecting other sounds
            }
            else
            {
                Debug.LogWarning("AudioSource is not assigned!");
            }
        }
    }
}
