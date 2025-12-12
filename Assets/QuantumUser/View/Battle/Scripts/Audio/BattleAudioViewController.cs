/// @file BattleAudioViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Audio,BattleAudioViewController} class which handles playing audio in Battle.
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
    /// Handles playing audio in Battle.
    /// </summary>
    public static class BattleAudioViewController
    {
        /// <summary>
        /// Starts the Battle music.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayMusic()
        {
            AudioManager.Instance.PlayMusic("Battle", MusicHandler.MusicSwitchType.Immediate);
        }

        /// <summary>
        /// Plays a given sound effect.
        /// </summary>
        ///
        /// <param name="effect">The BattleSoundFX which to play.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlaySoundFX(BattleSoundFX effect)
        {
            AudioManager.Instance.PlayBattleSfxAudio((BattleSFXNameTypes)effect);
        }
    }
}
