using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quantum;
using UnityEngine;

namespace Altzone.Scripts.ModelV2
{

    [CreateAssetMenu(menuName = "ALT-Zone/BattleMapReference", fileName = nameof(BattleMapReference))]
    public class BattleMapReference : ScriptableObject
    {
        [SerializeField, Header("All BattleMaps")]
        private List<BattleMap> _maps;

        private List<BattleMap> _validatedMaps = null;

        /// <summary>
        /// Get the list of validated BattleMaps.
        /// </summary>
        public List<BattleMap> Maps
        {
            get
            {
                if (_validatedMaps == null) ValidateMaps();
                return _validatedMaps;
            }
        }
        /// <summary>
        /// Validate that the data object has valid info within it. BattleMaps with missing Id or QuantumMap or duplicated data aren't validated.
        /// </summary>
        public void ValidateMaps()
        {
            var uniqueIds = new HashSet<string>();
            var uniqueNames = new HashSet<string>();
            var uniqueMap = new HashSet<Map>();

            if (_validatedMaps != null) return;
            List<BattleMap> maps = new();
            foreach (var map in _maps)
            {
                if (!map.IsValid()) continue;
                if (!uniqueIds.Add(map.MapId))
                {
                    Debug.LogError($"duplicate map ID {map}", map);
                    continue;
                }
                if (!uniqueNames.Add(map.MapName))
                {
                    Debug.LogError($"duplicate map Name {map}", map);
                }
                if (!uniqueMap.Add(map.Map))
                {
                    Debug.LogError($"duplicate map Object {map}", map);
                    continue;
                }
                maps.Add(map);
            }
            _validatedMaps = maps;
        }

        /// <summary>
        /// Fetch a BattleMap with a specific MapId from validated maps.
        /// </summary>
        /// <param name="id">MapId of the BattleMap</param>
        /// <returns>The desired BattleMap or NULL</returns>
        public BattleMap GetBattleMap(string id)
        {
            return Maps.FirstOrDefault(x => x.MapId == id);
        }
    }
}
