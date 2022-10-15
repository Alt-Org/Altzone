using System;
using TMPro;
using UnityEngine;

namespace Raid.Test.Scripts
{
    public class RaidManager_View : MonoBehaviour
    {
        [SerializeField] private TMP_Text _debugSeed;

        public void ResetView()
        {
            _debugSeed.text = string.Empty;
        }

        public void SetSeed(int seed) => _debugSeed.text = $"Seed {seed}";
    }
}