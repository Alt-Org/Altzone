/// @file BattleSoundFXViewController.cs
/// <summary>
/// Has a class BattleSoundFXViewController which handles playing sound effects.
/// </summary>
///
/// This script:<br/>
/// Handles playing sound effects.

using UnityEngine;
using Quantum;
using System;

namespace Battle.View.Audio
{
    /// <summary>
    /// <span class="brief-h">Sound FX view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles playing sound effects.
    /// </summary>
    public class BattleSoundFXViewController : MonoBehaviour
    {
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <value>[SerializeField] Reference to the <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioSource.html">AudioSource@u-exlink</a>.</value>
        [SerializeField] private AudioSource _audioSource;

        /// <value>[SerializeField] GoalHit sound effect data.</value>
        [SerializeField] private Effect _goalHit;

        /// <value>[SerializeField] SideWallHit sound effect data.</value>
        [SerializeField] private Effect _sideWallHit;

        /// <value>[SerializeField] WallBroken sound effect data.</value>
        [SerializeField] private Effect _wallBroken;

        /// @}

        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        /// <param name="effect">The BattleSoundFX which to play.</param>
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

        /// <summary>
        /// Private Serializable class for sound effect data.
        /// </summary>
        [Serializable]
        private class Effect
        {
            /// <value>[SerializeField] The sound effect's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioClip.html">AudiClip@u-exlink</a>.</value>
            public AudioClip Clip;

            /// <value>[SerializeField] The sound effect's volume scale.</value>
            [Range(0.0f, 10f)]
            public float VolumeScale = 1f;
        }
    }
}
