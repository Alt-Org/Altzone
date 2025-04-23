using UnityEngine;
using Quantum;
using System;

namespace Battle.View.Audio
{
    public class BattleSoundFXViewController : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        [SerializeField] private Effect _goalHit;
        [SerializeField] private Effect _sideWallHit;
        [SerializeField] private Effect _wallBroken;

        public void PlaySound(BattleSoundFX effect)
        {
            // Map SoundEffect enum to the correct AudioClip
            Effect fxRef = effect switch
            {
                BattleSoundFX.GoalHit     => _goalHit,
                BattleSoundFX.SideWallHit => _sideWallHit,
                BattleSoundFX.WallBroken  => _wallBroken,
                _ => null,
            };
            if(fxRef == null)
            {
                Debug.LogFormat("[{0}] Invalid SoundFX", nameof (BattleSoundFXViewController));
                return;
            }
            if (fxRef.Clip == null)
            {
                Debug.LogFormat("[{0}] Unhandled SoundFX: {1}", nameof (BattleSoundFXViewController), effect);
                return;
            }

            _audioSource.PlayOneShot(fxRef.Clip, fxRef.VolumeScale);
        }

        [Serializable]
        private class Effect
        {
            public AudioClip Clip;
            [Range(0.0f, 10f)]
            public float VolumeScale = 1f;
        }
    }
}
