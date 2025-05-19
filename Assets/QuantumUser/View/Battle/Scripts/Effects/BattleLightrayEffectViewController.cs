using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

namespace Battle.View.Effect
{
    public class BattleLightrayEffectViewController : MonoBehaviour
    {
        [SerializeField] private GameObject[] _lightrays;

        public void SpawnLightray(Vector2 position, float rotation, BattleLightrayColor color, BattleLightraySize size)
        {
            Debug.LogWarning(rotation);

            GameObject spawnedLightray = GameObject.Instantiate(_lightrays[(int)color * 3 + (int)size], transform);
            spawnedLightray.transform.position = new Vector3(position.x, 0, position.y);
            spawnedLightray.transform.rotation = Quaternion.Euler(90, rotation, 0);
            spawnedLightray.SetActive(true);
            _spawnedLightrays.Add(spawnedLightray);
        }

        private List<GameObject> _spawnedLightrays = new List<GameObject>();
    }
}
