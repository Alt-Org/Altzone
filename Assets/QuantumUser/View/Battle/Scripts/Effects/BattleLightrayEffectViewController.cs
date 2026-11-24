/// @file BattleLightrayEffectViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Effect,BattleLightrayEffectViewController} class which handles lightray effects visual functionality.
/// </summary>
///
/// This script:<br/>
/// Handles lightray effects visual functionality.

// System usings
using System;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Battle view usings
using Battle.View.Game;

namespace Battle.View.Effect
{
    /// <summary>
    /// <span class="brief-h">Lightray effect view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles lightray effects visual functionality.
    /// </summary>
    public class BattleLightrayEffectViewController : MonoBehaviour
    {
        /// @anchor BattleLightrayEffectViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Array containing references to team alpha's <see cref="Lightray">Lightrays</see>.</summary>
        /// Part of @ref BattleLightrayEffectViewController-SerializeFields "SerializeField variables"
        [Tooltip("References to team alpha's lightrays")]
        [SerializeField] private Lightray[] _lightraysTeamAlpha;

        /// <summary>[SerializeField] Array containing references to team beta's <see cref="Lightray">Lightrays</see>.</summary>
        /// Part of @ref BattleLightrayEffectViewController-SerializeFields "SerializeFields variables"
        [Tooltip("References to team beta's lightrays")]
        [SerializeField] private Lightray[] _lightraysTeamBeta;

        /// @}

        /// <summary>
        /// Activates a lightray associated with a %SoulWall segment that belongs to a team based on <paramref name="teamNumber"/> and <paramref name="wallNumber"/>.
        /// </summary>
        ///
        /// <param name="teamNumber">Team number of the desired team.</param>
        /// <param name="wallNumber">The wall number corresponding to the desired lightray.</param>
        public void ActivateLightray(BattleTeamNumber teamNumber, int wallNumber)
        {
            Lightray lightray = teamNumber switch
            {
                BattleTeamNumber.TeamAlpha => _lightraysTeamAlpha[wallNumber],
                BattleTeamNumber.TeamBeta  => _lightraysTeamBeta[wallNumber],

                _ => throw new ArgumentException(string.Format("{0} is not a valid team number for activating light rays", teamNumber))
            };

            Sprite lightraySprite = teamNumber == BattleGameViewController.LocalPlayerTeam ? lightray.Blue : lightray.Red;

            lightray.GameObject.GetComponent<SpriteRenderer>().sprite = lightraySprite;
            lightray.GameObject.SetActive(true);
        }

        /// <summary>
        /// Private helper struct for holding a reference to a lightray gameobject and it's sprite variants.
        /// </summary>
        [Serializable]
        private struct Lightray
        {
            /// <summary>Reference to the lightray gameobject.</summary>
            public GameObject GameObject;

            /// <summary>Reference to the blue sprite variant of the lightray.</summary>
            public Sprite Blue;

            /// <summary>Reference to the red sprite variant of the lightray.</summary>
            public Sprite Red;
        }
    }
}
