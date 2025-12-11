/// @file BattleSoundFXViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Audio,BattleSoundFXViewController} class which handles playing sound effects.
/// </summary>

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Altzone usings
using Altzone.Scripts.Audio;
using Assets.Altzone.Scripts.Reference_Sheets;

namespace Battle.View.Audio
{
    /// <summary>
    /// <span class="brief-h">Sound FX view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles playing sound effects.
    /// </summary>
    public class BattleSoundFXViewController : MonoBehaviour
    {
        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        ///
        /// <param name="effect">The BattleSoundFX which to play.</param>
        public void PlaySound(BattleSoundFX effect)
        {
            BattleSFXNameTypes effectName = (BattleSFXNameTypes)effect;

            AudioManager.Instance.PlayBattleSfxAudio(effectName);
        }
    }
}
