using System;
using Altzone.Scripts.Common;
using Quantum;
using UnityEngine;

namespace Altzone.Scripts.ModelV2
{

    [CreateAssetMenu(menuName = "ALT-Zone/BattleMap", fileName = nameof(BattleMap)+ "_ID")]
    public class BattleMap : ScriptableObject
    {
        [SerializeField]
        private string _mapId;
        [SerializeField]
        private string _mapName;
        [SerializeField]
        private Map _map = null;
        [SerializeField]
        private MapEmotionalSituation[] _emotionalSituations;

        public string MapId { get => _mapId; }
        public string MapName
        {
            get
            {
                if (string.IsNullOrEmpty(_mapName)) return _mapId;
                return _mapName;
            }
        }
        public Map Map { get => _map; }

        public MapEmotionalSituation[] EmotionalSituations { get => _emotionalSituations; }


        /// <summary>
        /// Check if the BattleMap has necessary valuesa set, with MapId and Map being mandatory.
        /// </summary>
        /// <returns>True if data is set. False if data is missing.</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(_mapId)) return false;
            if (_map == null) return false;
            return true;
        }
    }

    /// <summary>
    /// Serializable class to hold one of the map's emotional situation's emotion and name.
    /// </summary>
    [Serializable]
    public class MapEmotionalSituation
    {
        [SerializeField] public Emotion SituationEmotion;
        [SerializeField] public string SituationName;
    }
}
