/// @file BattleLightrayEffectViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Effect,BattleLightrayEffectViewController} class which handles lightray effects visual functionality.
/// </summary>
///
/// This script:<br/>
/// Handles lightray effects visual functionality.

// System usings
using System.Collections.Generic;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

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

        /// <summary>[SerializeField] Array of the red lightray <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a> references.</summary>
        /// @ref BattleLightrayEffectViewController-SerializeFields
        [SerializeField] private GameObject[] _lightraysRed;

        /// <summary>[SerializeField] Array of the blue lightray <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a> references.</summary>
        /// @ref BattleLightrayEffectViewController-SerializeFields
        [SerializeField] private GameObject[] _lightraysBlue;

        /// @}

        /// <summary>
        /// Activates the light ray which correspons to the wall number and color.
        /// </summary>
        ///
        /// <param name="wallNumber">The wall number which lightray to spawn.</param>
        /// <param name="color">The lightray's BattleLightrayColor which to spawn.</param>
        public void SpawnLightray(int wallNumber, BattleLightrayColor color)
        {
            switch (color)
            {
                case BattleLightrayColor.Red:
                    _lightraysRed[wallNumber].SetActive(true);
                    _spawnedLightrays.Add(_lightraysRed[wallNumber]);
                    break;
                case BattleLightrayColor.Blue:
                    _lightraysBlue[wallNumber].SetActive(true);
                    _spawnedLightrays.Add(_lightraysBlue[wallNumber]);
                    break;
            }
        }

        /// <value>List of currently active lightrays.</value>
        private List<GameObject> _spawnedLightrays = new();
    }
}
