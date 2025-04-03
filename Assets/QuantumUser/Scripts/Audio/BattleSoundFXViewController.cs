using UnityEngine;
using Quantum;

namespace Battle.View.Audio
{
    public class BattleSoundFXViewController : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        [SerializeField] private AudioClip _soulWallHitClip;
        [SerializeField] private AudioClip _goalHitClip;
        [SerializeField] private AudioClip _sideWallHitClip;
        [SerializeField] private AudioClip _wallBroken;

        public void PlaySound(BattleSoundFX effect)
        {
            // Map SoundEffect enum to the correct AudioClip
            AudioClip clip = effect switch
            {
                BattleSoundFX.SoulWallHit => _soulWallHitClip,
                BattleSoundFX.GoalHit     => _goalHitClip,
                BattleSoundFX.SideWallHit => _sideWallHitClip,
                BattleSoundFX.WallBroken  => _wallBroken,
                _ => null,
            };

            if (clip == null)
            {
                Debug.LogWarning("Unhandled sound effect: " + effect);
                return;
            }

            _audioSource.PlayOneShot(clip);
        }
    }
}
