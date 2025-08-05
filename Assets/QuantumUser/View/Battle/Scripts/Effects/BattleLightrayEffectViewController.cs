/// @file BattleLightrayEffectViewController.cs
/// <summary>
/// Handles lightray activation.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

namespace Battle.View.Effect
{
    /// <summary>
    /// Lightray effect <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour@u-exlink</a> script.<br/>
    /// Handles lightray activation.
    /// </summary>
    public class BattleLightrayEffectViewController : MonoBehaviour
    {
        /// <value>[SerializeField] An array of the red lightray gameObjects.</value>
        [SerializeField] private GameObject[] _lightraysRed;
        /// <value>[SerializeField] An array of the blue lightray gameObjects..</value>
        [SerializeField] private GameObject[] _lightraysBlue;

        /// <summary>
        /// Sets specified lightray active.
        /// </summary>
        /// <param name="wallNumber">The StoneCharacter part for which the matching lightray should be activated.</param>
        /// <param name="color">The color of the lightray to be activated.</param>
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

        private List<GameObject> _spawnedLightrays = new();
    }
}
