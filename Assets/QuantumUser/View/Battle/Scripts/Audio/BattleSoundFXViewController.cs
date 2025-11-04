/// @file BattleSoundFXViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Audio,BattleSoundFXViewController} class which handles playing sound effects.
/// </summary>
///
/// This script:<br/>
/// Handles playing sound effects.

// System usings
using System;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Altzone usings
using Altzone.Scripts.Audio;

namespace Battle.View.Audio
{
    /// <summary>
    /// <span class="brief-h">Sound FX view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles playing sound effects.
    /// </summary>
    public class BattleSoundFXViewController : MonoBehaviour
    {
        /// @anchor BattleSoundFXViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] GoalHit sound effect data.</summary>
        /// @ref BattleSoundFXViewController-SerializeFields
        [SerializeField] private Effect _goalHit;

        /// <summary>[SerializeField] SideWallHit sound effect data.</summary>
        /// @ref BattleSoundFXViewController-SerializeFields
        [SerializeField] private Effect _sideWallHit;

        /// <summary>[SerializeField] WallBroken sound effect data.</summary>
        /// @ref BattleSoundFXViewController-SerializeFields
        [SerializeField] private Effect _wallBroken;

        /// @}

        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        ///
        /// <param name="effect">The BattleSoundFX which to play.</param>
        public void PlaySound(BattleSoundFX effect)
        {
            // Map SoundEffect enum to the correct effect
            Effect effectRef = effect switch
            {
                BattleSoundFX.GoalHit     => _goalHit,
                BattleSoundFX.SideWallHit => _sideWallHit,
                BattleSoundFX.WallBroken  => _wallBroken,

                _ => null,
            };

            if (!effectRef.Active) return;
            AudioManager.Instance.PlaySfxAudio("Battle_Common", effectRef.Name);
        }

        /// <summary>
        /// Private Serializable class for sound effect data.
        /// </summary>
        [Serializable]
        private class Effect
        {
            public bool Active = false;
            public string Name;
        }
    }
}
