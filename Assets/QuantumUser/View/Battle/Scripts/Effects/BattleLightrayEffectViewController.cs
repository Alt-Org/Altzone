using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

namespace Battle.View.Effect
{
    public class BattleLightrayEffectViewController : MonoBehaviour
    {
        [SerializeField] private GameObject[] _lightrays;
        [SerializeField] private GameObject[] _lightrayPositionsRed;
        [SerializeField] private GameObject[] _lightrayPositionsBlue;

        public void SpawnLightray(int wallNumber, float rotation, BattleLightrayColor color, BattleLightraySize size)
        {
            GameObject spawnedLightray = GameObject.Instantiate(_lightrays[(int)color * 3 + (int)size], transform);

            switch (color)
            {
                case BattleLightrayColor.Red:
                    spawnedLightray.transform.SetPositionAndRotation(
                        _lightrayPositionsRed[wallNumber].transform.position,
                        Quaternion.Euler(90, rotation, 0)
                    );
                    break;
                case BattleLightrayColor.Blue:
                    spawnedLightray.transform.SetPositionAndRotation(
                        _lightrayPositionsBlue[wallNumber].transform.position,
                        Quaternion.Euler(90, rotation, 0)
                    );
                    break;
            }

            spawnedLightray.SetActive(true);
            _spawnedLightrays.Add(spawnedLightray);
        }

        private List<GameObject> _spawnedLightrays = new();
    }
}
