using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

namespace Battle.View.Effect
{
    public class BattleLightrayEffectViewController : MonoBehaviour
    {
        [SerializeField] private GameObject[] _lightraysRed;
        [SerializeField] private GameObject[] _lightraysBlue;

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
