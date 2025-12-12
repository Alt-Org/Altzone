/// @file BattleSoundFXController.cs
/// <summary>
/// Contains @cref{Battle.View.Audio,BattleSoundFXController} class which handles playing sound effects.
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;

// Altzone usings
using Altzone.Scripts.Audio;
using Assets.Altzone.Scripts.Reference_Sheets;

namespace Battle.View.Audio
{
    /// <summary>
    /// Handles playing sound effects.
    /// </summary>
    public static class BattleSoundFXController
    {
        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        ///
        /// <param name="effect">The BattleSoundFX which to play.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlaySound(BattleSoundFX effect)
        {
            AudioManager.Instance.PlayBattleSfxAudio((BattleSFXNameTypes)effect);
        }
    }
}
