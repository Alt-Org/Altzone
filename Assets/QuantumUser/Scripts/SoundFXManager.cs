using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuantumUser.Scripts
{
    public class SoundFXManager : MonoBehaviour
    {
        public static SoundFXManager instance;

        [SerializeField] private AudioSource audioSource; // Reference to a preconfigured AudioSource

        [SerializeField] private AudioClip soulWallHitClip;
        [SerializeField] private AudioClip goalHitClip;
        [SerializeField] private AudioClip sideWallHitClip;
        [SerializeField] private AudioClip wallBroken;
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
            AudioClip clip = null;

            // Map SoundEffect enum to the correct AudioClip
            switch (e.SoundEffect)
            {
                case SoundEffect.SoulWallHit:
                    clip = soulWallHitClip;
                    break;
                case SoundEffect.GoalHit:
                    clip = goalHitClip;
                    break;
                case SoundEffect.SideWallHit:
                    clip = sideWallHitClip;
                    break;
                case SoundEffect.WallBroken:
                    clip = wallBroken;
                    break;
                default:
                    Debug.LogWarning("Unhandled sound effect: " + e.SoundEffect);
                    return;
            }

            if (clip != null)
            {
                PlaySoundFXclip(clip);
            }
        }

        private void PlaySoundFXclip(AudioClip clip)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip); // Play the sound without affecting other sounds
            }
            else
            {
                Debug.LogWarning("AudioSource is not assigned!");
            }
        }
    }
}
